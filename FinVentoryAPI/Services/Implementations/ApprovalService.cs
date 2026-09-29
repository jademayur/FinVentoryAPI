using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.ApprovalDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class ApprovalService : IApprovalService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public ApprovalService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        // ════════════════════════════════════════════════════
        // GET LEVELS
        // ════════════════════════════════════════════════════
        public async Task<List<ApprovalLevelDto>> GetLevelsAsync(string documentType)
        {
            var companyId = _common.GetCompanyId();

            return await _context.ApprovalLevels
                .Where(x => x.CompanyId == companyId && x.DocumentType == documentType)
                .OrderBy(x => x.LevelNumber)
                .Select(x => new ApprovalLevelDto
                {
                    ApprovalLevelId = x.ApprovalLevelId,
                    DocumentType = x.DocumentType,
                    LevelNumber = x.LevelNumber,
                    LevelName = x.LevelName,
                    MaxAmount = x.MaxAmount,
                    RoleId = x.RoleId,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        // ════════════════════════════════════════════════════
        // UPSERT LEVELS (Replace all levels for a document type)
        // ════════════════════════════════════════════════════
        public async Task UpsertLevelsAsync(BulkUpsertApprovalLevelDto dto)
        {
            var companyId = _common.GetCompanyId();

            // Remove existing levels for this document type
            var existing = await _context.ApprovalLevels
                .Where(x => x.CompanyId == companyId && x.DocumentType == dto.DocumentType)
                .ToListAsync();

            _context.ApprovalLevels.RemoveRange(existing);

            // Add new levels
            foreach (var level in dto.Levels)
            {
                _context.ApprovalLevels.Add(new ApprovalLevel
                {
                    CompanyId = companyId,
                    DocumentType = dto.DocumentType,
                    LevelNumber = level.LevelNumber,
                    LevelName = level.LevelName,
                    MaxAmount = level.MaxAmount,
                    RoleId = level.RoleId,
                    IsActive = level.IsActive,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════
        // CHECK IF APPROVAL IS REQUIRED
        // ════════════════════════════════════════════════════
        public async Task<bool> IsApprovalRequiredAsync(int companyId, string documentType)
        {
            // Check master toggle first
            var masterValue = await _context.CompanyConfigs
                .Where(x => x.CompanyId == companyId && x.ConfigKey == "ApprovalSystemEnabled")
                .Select(x => x.ConfigValue)
                .FirstOrDefaultAsync();

            if (masterValue?.ToLower() != "true")
                return false;

            var configKey = $"ApprovalRequired_{documentType}";
            var value = await _context.CompanyConfigs
                .Where(x => x.CompanyId == companyId && x.ConfigKey == configKey)
                .Select(x => x.ConfigValue)
                .FirstOrDefaultAsync();

            return value?.ToLower() == "true";
        }

        // ════════════════════════════════════════════════════
        // GET APPLICABLE LEVEL (based on amount)
        // ════════════════════════════════════════════════════
        private async Task<ApprovalLevel?> GetApplicableLevelAsync(int companyId, string documentType, decimal? amount)
        {
            var levels = await _context.ApprovalLevels
                .Where(x => x.CompanyId == companyId
                         && x.DocumentType == documentType
                         && x.IsActive)
                .OrderBy(x => x.LevelNumber)
                .ToListAsync();

            if (!levels.Any())
                return null;

            // If amount provided, find the first level where amount <= MaxAmount
            // If no MaxAmount limit, use the level
            if (amount.HasValue)
            {
                return levels
                    .Where(x => x.MaxAmount == null || amount.Value <= x.MaxAmount)
                    .OrderBy(x => x.LevelNumber)
                    .FirstOrDefault();
            }

            // No amount check - return first active level
            return levels.First();
        }

        // ════════════════════════════════════════════════════
        // SUBMIT FOR APPROVAL
        // ════════════════════════════════════════════════════
        public async Task<bool> SubmitForApprovalAsync(string documentType, int documentId)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            // Get the document's net total
            var netTotal = await GetDocumentNetTotalAsync(documentType, documentId, companyId);
            if (netTotal == null)
                throw new Exception($"Document not found: {documentType} #{documentId}");

            // Get applicable level based on amount
            var firstLevel = await GetApplicableLevelAsync(companyId, documentType, netTotal);
            if (firstLevel == null)
                throw new Exception($"No approval levels configured for {documentType}. Please configure approval levels first.");

            // Update document status
            var main = await GetMainEntityAsync(documentType, documentId, companyId);
            if (main == null)
                throw new Exception($"Document not found: {documentType} #{documentId}");

            var currentStatus = GetEntityStatus(main);
            if (currentStatus != "Draft")
                throw new Exception($"Only Draft documents can be submitted for approval. Current status: {currentStatus}");

            SetEntityStatus(main, $"PendingApproval_L{firstLevel.LevelNumber}");
            await _context.SaveChangesAsync();

            // Log the submission
            await LogApprovalAsync(companyId, documentType, documentId,
                firstLevel.LevelNumber, "Submit", userId, null);

            return true;
        }

        // ════════════════════════════════════════════════════
        // APPROVE
        // ════════════════════════════════════════════════════
        public async Task<bool> ApproveAsync(string documentType, int documentId, string? remarks)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await GetMainEntityAsync(documentType, documentId, companyId);
            if (main == null)
                throw new Exception($"Document not found: {documentType} #{documentId}");

            var currentStatus = GetEntityStatus(main);
            if (!currentStatus.StartsWith("PendingApproval_L"))
                throw new Exception($"Document is not pending approval. Current status: {currentStatus}");

            // Extract current level number from status
            var currentLevelStr = currentStatus.Replace("PendingApproval_L", "");
            if (!int.TryParse(currentLevelStr, out var currentLevelNumber))
                throw new Exception("Invalid approval status format");

            // Verify user has CanApprove permission for this document type
            if (!await CanUserApproveAsync(userId, documentType))
                throw new Exception($"You don't have approval permission for {documentType}.");

            // Find next level
            var nextLevel = await _context.ApprovalLevels
                .Where(x => x.CompanyId == companyId
                         && x.DocumentType == documentType
                         && x.LevelNumber > currentLevelNumber
                         && x.IsActive)
                .OrderBy(x => x.LevelNumber)
                .FirstOrDefaultAsync();

            // Update status
            if (nextLevel != null)
            {
                SetEntityStatus(main, $"PendingApproval_L{nextLevel.LevelNumber}");
            }
            else
            {
                // Last level approved - confirm the document
                SetEntityStatus(main, "Confirmed");
            }

            await _context.SaveChangesAsync();

            // Log the approval
            await LogApprovalAsync(companyId, documentType, documentId,
                currentLevelNumber, "Approve", userId, remarks);

            return true;
        }

        // ════════════════════════════════════════════════════
        // REJECT
        // ════════════════════════════════════════════════════
        public async Task<bool> RejectAsync(string documentType, int documentId, string remarks)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            if (string.IsNullOrWhiteSpace(remarks))
                throw new Exception("Rejection remarks are required.");

            var main = await GetMainEntityAsync(documentType, documentId, companyId);
            if (main == null)
                throw new Exception($"Document not found: {documentType} #{documentId}");

            var currentStatus = GetEntityStatus(main);
            if (!currentStatus.StartsWith("PendingApproval_L"))
                throw new Exception($"Document is not pending approval. Current status: {currentStatus}");

            var currentLevelStr = currentStatus.Replace("PendingApproval_L", "");
            if (!int.TryParse(currentLevelStr, out var currentLevelNumber))
                throw new Exception("Invalid approval status format");

            // Verify user has CanApprove permission for this document type
            if (!await CanUserApproveAsync(userId, documentType))
                throw new Exception($"You don't have approval permission for {documentType}.");

            // Reject - send back to Draft
            SetEntityStatus(main, "Rejected");
            await _context.SaveChangesAsync();

            // Log the rejection
            await LogApprovalAsync(companyId, documentType, documentId,
                currentLevelNumber, "Reject", userId, remarks);

            return true;
        }

        // ════════════════════════════════════════════════════
        // CHECK IF USER CAN APPROVE THIS DOCUMENT TYPE
        // ════════════════════════════════════════════════════
        private async Task<bool> CanUserApproveAsync(int userId, string documentType)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Get the MenuItem that matches this document type (by MenuName or ControllerName)
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(x => x.IsActive && 
                    (x.MenuName == documentType || x.ControllerName == documentType));

            if (menuItem == null) return false;

            // Check if user's role has CanApprove for this MenuItem
            var hasPermission = await _context.RoleRights
                .AnyAsync(x => x.RoleId == user.RoleId 
                            && x.MenuItemId == menuItem.MenuItemId 
                            && x.CanApprove);

            return hasPermission;
        }

        // ════════════════════════════════════════════════════
        // GET PENDING APPROVALS
        // ════════════════════════════════════════════════════
        public async Task<List<ApprovalQueueDto>> GetPendingApprovalsAsync(string? documentType)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            // Get all document types where user has CanApprove permission
            var allDocTypes = new[] { "SalesQuotation", "SalesOrder", "GoodsDelivery", "SalesInvoice", "SalesReturn",
                "PurchaseOrder", "GRN", "PurchaseInvoice", "PurchaseReturn",
                "StockTransfer", "StockAdjustment",
                "ProductionIssue", "ProductionReceipt",
                "JobWorkIssue", "JobWorkReceipt",
                "JournalEntry", "CashBankEntry", "IncomingPayment", "OutgoingPayment" };

            var result = new List<ApprovalQueueDto>();

            foreach (var docType in allDocTypes)
            {
                if (documentType != null && docType != documentType)
                    continue;

                // Check if user has CanApprove permission for this document type
                if (!await CanUserApproveAsync(userId, docType))
                    continue;

                // Get approval levels for this document type
                var levels = await _context.ApprovalLevels
                    .Where(x => x.CompanyId == companyId 
                             && x.DocumentType == docType 
                             && x.IsActive)
                    .OrderBy(x => x.LevelNumber)
                    .ToListAsync();

                foreach (var level in levels)
                {
                    var statusPattern = $"PendingApproval_L{level.LevelNumber}";
                    var pendingDocs = await GetDocumentsByStatusAsync(docType, companyId, statusPattern);

                    foreach (var doc in pendingDocs)
                    {
                        result.Add(new ApprovalQueueDto
                        {
                            DocumentType = docType,
                            DocumentId = doc.Id,
                            DocumentNo = doc.DocumentNo,
                            NetTotal = doc.NetTotal,
                            CurrentLevel = level.LevelNumber,
                            CurrentLevelName = level.LevelName,
                            Status = statusPattern,
                            CreatedBy = doc.CreatedBy,
                            CreatedDate = doc.CreatedDate
                        });
                    }
                }
            }

            return result.OrderByDescending(x => x.CreatedDate).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET HISTORY
        // ════════════════════════════════════════════════════
        public async Task<ApprovalHistoryDto> GetHistoryAsync(string documentType, int documentId)
        {
            var companyId = _common.GetCompanyId();

            var logs = await _context.ApprovalLogs
                .Where(x => x.CompanyId == companyId
                         && x.DocumentType == documentType
                         && x.DocumentId == documentId)
                .OrderBy(x => x.ActionDate)
                .ToListAsync();

            var result = new ApprovalHistoryDto
            {
                DocumentType = documentType,
                DocumentId = documentId,
                Logs = logs.Select(l => new ApprovalLogEntryDto
                {
                    LevelNumber = l.LevelNumber,
                    Action = l.Action,
                    Remarks = l.Remarks,
                    ActionDate = l.ActionDate
                }).ToList()
            };

            // Get level names
            foreach (var log in result.Logs)
            {
                var level = await _context.ApprovalLevels
                    .Where(x => x.CompanyId == companyId
                             && x.DocumentType == documentType
                             && x.LevelNumber == log.LevelNumber)
                    .FirstOrDefaultAsync();
                log.LevelName = level?.LevelName ?? $"Level {log.LevelNumber}";
            }

            return result;
        }

        // ════════════════════════════════════════════════════
        // GET CURRENT APPROVAL STATUS
        // ════════════════════════════════════════════════════
        public async Task<string?> GetCurrentApprovalStatusAsync(int companyId, string documentType, int documentId)
        {
            var main = await GetMainEntityAsync(documentType, documentId, companyId);
            if (main == null) return null;
            return GetEntityStatus(main);
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        private async Task<int?> GetUserRoleIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.RoleId;
        }

        private async Task<decimal?> GetDocumentNetTotalAsync(string documentType, int documentId, int companyId)
        {
            return documentType switch
            {
                "SalesInvoice" => await _context.SalesInvoiceMains
                    .Where(x => x.InvoiceId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "PurchaseInvoice" => await _context.PurchaseInvoiceMains
                    .Where(x => x.InvoiceId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "SalesOrder" => await _context.SalesOrderMains
                    .Where(x => x.OrderId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "PurchaseOrder" => await _context.PurchaseOrderMains
                    .Where(x => x.OrderId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "SalesQuotation" => await _context.SalesQuotationMains
                    .Where(x => x.QuotationId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "GRN" => await _context.GRNMains
                    .Where(x => x.GRNId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "GoodsDelivery" => await _context.GoodsDeliveryMains
                    .Where(x => x.DeliveryId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "IncomingPayment" => await _context.IncomingPaymentMains
                    .Where(x => x.PaymentId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.TotalAmount)
                    .FirstOrDefaultAsync(),

                "OutgoingPayment" => await _context.OutgoingPaymentMains
                    .Where(x => x.PaymentId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.TotalAmount)
                    .FirstOrDefaultAsync(),

                "SalesReturn" => await _context.SalesReturnMains
                    .Where(x => x.ReturnId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "PurchaseReturn" => await _context.PurchaseReturnMains
                    .Where(x => x.ReturnId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => x.NetTotal)
                    .FirstOrDefaultAsync(),

                "StockTransfer" => await _context.StockTransferMains
                    .Where(x => x.TransferId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => (decimal?)0)
                    .FirstOrDefaultAsync(),

                "StockAdjustment" => await _context.StockAdjustmentMains
                    .Where(x => x.AdjustmentId == documentId && x.CompanyId == companyId && !x.IsDeleted)
                    .Select(x => (decimal?)0)
                    .FirstOrDefaultAsync(),

                _ => null
            };
        }

        private async Task<object?> GetMainEntityAsync(string documentType, int documentId, int companyId)
        {
            return documentType switch
            {
                "SalesInvoice" => await _context.SalesInvoiceMains
                    .FirstOrDefaultAsync(x => x.InvoiceId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "PurchaseInvoice" => await _context.PurchaseInvoiceMains
                    .FirstOrDefaultAsync(x => x.InvoiceId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "SalesOrder" => await _context.SalesOrderMains
                    .FirstOrDefaultAsync(x => x.OrderId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "PurchaseOrder" => await _context.PurchaseOrderMains
                    .FirstOrDefaultAsync(x => x.OrderId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "SalesQuotation" => await _context.SalesQuotationMains
                    .FirstOrDefaultAsync(x => x.QuotationId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "GRN" => await _context.GRNMains
                    .FirstOrDefaultAsync(x => x.GRNId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "GoodsDelivery" => await _context.GoodsDeliveryMains
                    .FirstOrDefaultAsync(x => x.DeliveryId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "IncomingPayment" => await _context.IncomingPaymentMains
                    .FirstOrDefaultAsync(x => x.PaymentId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "OutgoingPayment" => await _context.OutgoingPaymentMains
                    .FirstOrDefaultAsync(x => x.PaymentId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "SalesReturn" => await _context.SalesReturnMains
                    .FirstOrDefaultAsync(x => x.ReturnId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "PurchaseReturn" => await _context.PurchaseReturnMains
                    .FirstOrDefaultAsync(x => x.ReturnId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "StockTransfer" => await _context.StockTransferMains
                    .FirstOrDefaultAsync(x => x.TransferId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "StockAdjustment" => await _context.StockAdjustmentMains
                    .FirstOrDefaultAsync(x => x.AdjustmentId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "ProductionIssue" => await _context.ProductionIssueMains
                    .FirstOrDefaultAsync(x => x.ProductionIssueId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "ProductionReceipt" => await _context.ProductionReceiptMains
                    .FirstOrDefaultAsync(x => x.ProductionReceiptId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "JobWorkIssue" => await _context.JobWorkIssueMains
                    .FirstOrDefaultAsync(x => x.JobWorkIssueId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                "JobWorkReceipt" => await _context.JobWorkReceiptMains
                    .FirstOrDefaultAsync(x => x.JobWorkReceiptId == documentId && x.CompanyId == companyId && !x.IsDeleted),

                _ => null
            };
        }

        private string GetEntityStatus(object entity)
        {
            return entity switch
            {
                SalesInvoiceMain x => x.Status,
                PurchaseInvoiceMain x => x.Status,
                SalesOrderMain x => x.Status,
                PurchaseOrderMain x => x.Status,
                SalesQuotationMain x => x.Status,
                GRNMain x => x.Status,
                GoodsDeliveryMain x => x.Status,
                IncomingPaymentMain x => x.Status,
                OutgoingPaymentMain x => x.Status,
                StockTransferMain x => x.Status,
                StockAdjustmentMain x => x.Status,
                SalesReturnMain x => x.Status,
                PurchaseReturnMain x => x.Status,
                ProductionIssueMain x => x.Status,
                ProductionReceiptMain x => x.Status,
                JobWorkIssueMain x => x.Status,
                JobWorkReceiptMain x => x.Status,
                _ => throw new Exception("Unknown document type")
            };
        }

        private void SetEntityStatus(object entity, string status)
        {
            switch (entity)
            {
                case SalesInvoiceMain x: x.Status = status; break;
                case PurchaseInvoiceMain x: x.Status = status; break;
                case SalesOrderMain x: x.Status = status; break;
                case PurchaseOrderMain x: x.Status = status; break;
                case SalesQuotationMain x: x.Status = status; break;
                case GRNMain x: x.Status = status; break;
                case GoodsDeliveryMain x: x.Status = status; break;
                case IncomingPaymentMain x: x.Status = status; break;
                case OutgoingPaymentMain x: x.Status = status; break;
                case StockTransferMain x: x.Status = status; break;
                case StockAdjustmentMain x: x.Status = status; break;
                case SalesReturnMain x: x.Status = status; break;
                case PurchaseReturnMain x: x.Status = status; break;
                case ProductionIssueMain x: x.Status = status; break;
                case ProductionReceiptMain x: x.Status = status; break;
                case JobWorkIssueMain x: x.Status = status; break;
                case JobWorkReceiptMain x: x.Status = status; break;
                default: throw new Exception("Unknown document type");
            }
        }

        private async Task<List<DocumentInfo>> GetDocumentsByStatusAsync(string documentType, int companyId, string status)
        {
            return documentType switch
            {
                "SalesInvoice" => await _context.SalesInvoiceMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.InvoiceId, DocumentNo = x.InvoiceNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "PurchaseInvoice" => await _context.PurchaseInvoiceMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.InvoiceId, DocumentNo = x.InvoiceNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "SalesOrder" => await _context.SalesOrderMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.OrderId, DocumentNo = x.OrderNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "PurchaseOrder" => await _context.PurchaseOrderMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.OrderId, DocumentNo = x.OrderNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "SalesQuotation" => await _context.SalesQuotationMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.QuotationId, DocumentNo = x.QuotationNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "GRN" => await _context.GRNMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.GRNId, DocumentNo = x.GRNNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "GoodsDelivery" => await _context.GoodsDeliveryMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.DeliveryId, DocumentNo = x.DeliveryNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "IncomingPayment" => await _context.IncomingPaymentMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.PaymentId, DocumentNo = x.PaymentNo, NetTotal = x.TotalAmount, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "OutgoingPayment" => await _context.OutgoingPaymentMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.PaymentId, DocumentNo = x.PaymentNo, NetTotal = x.TotalAmount, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "SalesReturn" => await _context.SalesReturnMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.ReturnId, DocumentNo = x.ReturnNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "PurchaseReturn" => await _context.PurchaseReturnMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.ReturnId, DocumentNo = x.ReturnNo, NetTotal = x.NetTotal, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "StockTransfer" => await _context.StockTransferMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.TransferId, DocumentNo = x.TransferNo, NetTotal = 0, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                "StockAdjustment" => await _context.StockAdjustmentMains
                    .Where(x => x.CompanyId == companyId && !x.IsDeleted && x.Status == status)
                    .Select(x => new DocumentInfo { Id = x.AdjustmentId, DocumentNo = x.AdjustmentNo, NetTotal = 0, CreatedBy = "", CreatedDate = x.CreatedDate })
                    .ToListAsync(),

                _ => new List<DocumentInfo>()
            };
        }

        private async Task LogApprovalAsync(int companyId, string documentType, int documentId,
            int levelNumber, string action, int userId, string? remarks)
        {
            _context.ApprovalLogs.Add(new ApprovalLog
            {
                CompanyId = companyId,
                DocumentType = documentType,
                DocumentId = documentId,
                LevelNumber = levelNumber,
                Action = action,
                UserId = userId,
                Remarks = remarks,
                ActionDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        private class DocumentInfo
        {
            public int Id { get; set; }
            public string DocumentNo { get; set; } = string.Empty;
            public decimal? NetTotal { get; set; }
            public string CreatedBy { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
        }
    }
}
