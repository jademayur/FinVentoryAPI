using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.BusinessPartnerDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public BusinessPartnerService(AppDbContext context, Common common)
        {
            _common = common;
            _context = context;
        }

        // ────────────────────────────────────────────────────
        // CREATE
        // ────────────────────────────────────────────────────
        public async Task<BusinessPartnerResponseDto> CreateAsync(CreateBusinessPartnerDto dto)
        {
            var companyId = _common.GetCompanyId();

            var duplicate = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerName.ToLower() == dto.BPName.ToLower() &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Business Partner already exists.");

            var account = new Account
            {
                AccountName = dto.BPName,
                AccountCode = dto.BPCode,
                AccountGroupId = dto.AccountGroupId,
                AccountType = AccountType.General,
                BookType = null,
                BookSubType = null,
                CompanyId = companyId,
                CreatedBy = _common.GetUserId(),
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var bp = new BusinessPartner
            {
                CompanyId = companyId,
                BusinessPartnerCode = dto.BPCode,
                BusinessPartnerName = dto.BPName,
                PrintName = dto.PrintName,
                Type = dto.BPType,
                Mobile = dto.Mobile,
                Email = dto.Email,
                CreditLimit = (decimal)dto.CreditLimit,
                CreditDays = (int)dto.CreditDays,
                AccountGroupId = dto.AccountGroupId,
                AccountId = account.AccountId,
                CreatedBy = _common.GetUserId(),
                DefaultPriceType = dto.DefaultPriceType
            };

            _context.BusinessPartners.Add(bp);
            await _context.SaveChangesAsync();

            if (dto.BPAddresses != null && dto.BPAddresses.Any())
            {
                var addresses = dto.BPAddresses.Select(p => new BusinessPartnerAddress
                {
                    BusinessPartnerId = bp.BusinessPartnerId,
                    Type = p.Type,
                    AddressLine1 = p.AddressLine1,
                    AddressLine2 = p.AddressLine2,
                    City = p.City,
                    State = p.State,
                    Country = p.Country,
                    Pincode = p.Pincode,
                    GSTType = p.GSTType,
                    GSTNo = p.GSTNo,
                    IsDefault = p.IsDefault,
                }).ToList();

                _context.BusinessPartnerAddresses.AddRange(addresses);
                await _context.SaveChangesAsync();
            }

            if (dto.BPContacts != null && dto.BPContacts.Any())
            {
                var contacts = dto.BPContacts.Select(p => new BusinessPartnerContact
                {
                    BusinessPartnerId = bp.BusinessPartnerId,
                    Name = p.Name,
                    Mobile = p.Mobile,
                    Email = p.Email,
                    Designation = p.Designation,
                    IsPrimary = p.IsPrimary,
                }).ToList();

                _context.BusinessPartnerContacts.AddRange(contacts);
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(bp.BusinessPartnerId);
        }

        // ────────────────────────────────────────────────────
        // UPDATE
        // ────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateBusinessPartnerDto dto)
        {
            var companyId = _common.GetCompanyId();

            var bp = await _context.BusinessPartners
                .Include(x => x.BPAddresses)
                .Include(x => x.BPContacts)
                .FirstOrDefaultAsync(x =>
                    x.BusinessPartnerId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (bp == null)
                return false;

            var duplicate = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerName.ToLower() == dto.BPName.ToLower() &&
                    x.BusinessPartnerId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Business Partner with the same name already exists.");

            bp.BusinessPartnerCode = dto.BPCode;
            bp.BusinessPartnerName = dto.BPName;
            bp.PrintName = dto.PrintName;
            bp.Type = dto.BPType;
            bp.Mobile = dto.Mobile;
            bp.Email = dto.Email;
            bp.CreditLimit = (decimal)dto.CreditLimit;
            bp.CreditDays = (int)dto.CreditDays;
            bp.AccountGroupId = dto.AccountGroupId;
            bp.AccountId = dto.AccountId;
            bp.IsActive = dto.IsActive;
            bp.ModifiedBy = _common.GetUserId();
            bp.ModifiedDate = DateTime.UtcNow;
            bp.DefaultPriceType = dto.DefaultPriceType;

            if (dto.BPAddresses != null)
            {
                _context.BusinessPartnerAddresses.RemoveRange(bp.BPAddresses);

                var newAddresses = dto.BPAddresses.Select(p => new BusinessPartnerAddress
                {
                    BusinessPartnerId = bp.BusinessPartnerId,
                    Type = p.Type,
                    AddressLine1 = p.AddressLine1,
                    AddressLine2 = p.AddressLine2,
                    City = p.City,
                    State = p.State,
                    Country = p.Country,
                    Pincode = p.Pincode,
                    GSTType = p.GSTType,
                    GSTNo = p.GSTNo,
                    IsDefault = p.IsDefault,
                });

                await _context.BusinessPartnerAddresses.AddRangeAsync(newAddresses);
            }

            if (dto.BPContacts != null)
            {
                _context.BusinessPartnerContacts.RemoveRange(bp.BPContacts);

                var newContacts = dto.BPContacts.Select(p => new BusinessPartnerContact
                {
                    BusinessPartnerId = bp.BusinessPartnerId,
                    Name = p.Name,
                    Mobile = p.Mobile,
                    Email = p.Email,
                    Designation = p.Designation,
                    IsPrimary = p.IsPrimary,
                });

                await _context.BusinessPartnerContacts.AddRangeAsync(newContacts);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────
        // DELETE
        // ────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var item = await _context.BusinessPartners
                .FirstOrDefaultAsync(x =>
                    x.BusinessPartnerId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (item == null)
                return false;

            item.IsDeleted = true;
            item.IsActive = false;
            item.ModifiedBy = _common.GetUserId();
            item.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────
        // GET BY ID
        // ────────────────────────────────────────────────────
        public async Task<BusinessPartnerResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var bp = await _context.BusinessPartners
                .Include(x => x.BPAddresses)
                .Include(x => x.BPContacts)
                .Include(x => x.accountGroup)
                .FirstOrDefaultAsync(x =>
                    x.BusinessPartnerId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (bp == null)
                return null;

            return MapBP(bp);
        }

        // ────────────────────────────────────────────────────
        // GET ALL
        // ────────────────────────────────────────────────────
        public async Task<List<BusinessPartnerResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var businessPartners = await _context.BusinessPartners
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .ToListAsync();

            return businessPartners.Select(MapBP).ToList();
        }

        // ────────────────────────────────────────────────────
        // GET PAGED
        // ────────────────────────────────────────────────────
        public async Task<PagedResponseDto<BusinessPartnerResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.BusinessPartners
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.accountGroup)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.BusinessPartnerName.ToLower().Contains(search) ||
                    x.BusinessPartnerCode.ToLower().Contains(search));
            }

            // FILTERS
            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("type"))
                {
                    var type = ((JsonElement)request.Filters["type"]).GetInt32();
                    query = query.Where(x => (int)x.Type == type);
                }

                if (request.Filters.ContainsKey("isActive"))
                {
                    var isActive = ((JsonElement)request.Filters["isActive"]).GetBoolean();
                    query = query.Where(x => x.IsActive == isActive);
                }
            }

            // SORTING
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                switch (sort.Column.ToLower())
                {
                    case "bpname":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.BusinessPartnerName)
                            : query.OrderBy(x => x.BusinessPartnerName);
                        break;
                    case "type":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.Type)
                            : query.OrderBy(x => x.Type);
                        break;
                    case "isactive":
                        query = sort.Direction == "desc"
                            ? query.OrderByDescending(x => x.IsActive)
                            : query.OrderBy(x => x.IsActive);
                        break;
                    default:
                        query = query.OrderBy(x => x.BusinessPartnerName);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.BusinessPartnerName);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponseDto<BusinessPartnerResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapBP).ToList()
            };
        }

        // ────────────────────────────────────────────────────
        // GET CUSTOMERS
        // ────────────────────────────────────────────────────
        public async Task<List<BusinessPartnerResponseDto>> GetCustomersAsync()
        {
            var companyId = _common.GetCompanyId();

            var businessPartners = await _context.BusinessPartners
                .Where(x =>
                    x.CompanyId == companyId &&
                    !x.IsDeleted &&
                    (x.Type == BusinessPartnerType.Customer || x.Type == BusinessPartnerType.Both))
                .ToListAsync();

            return businessPartners.Select(MapBP).ToList();
        }

        // ────────────────────────────────────────────────────
        // GET SUPPLIERS
        // ────────────────────────────────────────────────────
        public async Task<List<BusinessPartnerResponseDto>> GetSuppliersAsync()
        {
            var companyId = _common.GetCompanyId();

            var businessPartners = await _context.BusinessPartners
                .Where(x =>
                    x.CompanyId == companyId &&
                    !x.IsDeleted &&
                    (x.Type == BusinessPartnerType.Supplier || x.Type == BusinessPartnerType.Both))
                .ToListAsync();

            return businessPartners.Select(MapBP).ToList();
        }

        // ────────────────────────────────────────────────────
        // GET ADDRESSES BY BP
        // ────────────────────────────────────────────────────
        public async Task<List<BPAddressResponseDto>> GetAddressesByBPAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();
            await ValidateBPAsync(businessPartnerId, companyId);

            var addresses = await _context.BusinessPartnerAddresses
                .Where(x => x.BusinessPartnerId == businessPartnerId)
                .ToListAsync();

            return MapAddresses(addresses);
        }

        // ────────────────────────────────────────────────────
        // GET BILL ADDRESSES
        // ────────────────────────────────────────────────────
        public async Task<List<BPAddressResponseDto>> GetBillAddressesByBPAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();
            await ValidateBPAsync(businessPartnerId, companyId);

            var addresses = await _context.BusinessPartnerAddresses
                .Where(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    (x.Type == AddressType.Billing || x.Type == AddressType.Common))
                .ToListAsync();

            return MapAddresses(addresses);
        }

        // ────────────────────────────────────────────────────
        // GET SHIP ADDRESSES
        // ────────────────────────────────────────────────────
        public async Task<List<BPAddressResponseDto>> GetShipAddressesByBPAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();
            await ValidateBPAsync(businessPartnerId, companyId);

            var addresses = await _context.BusinessPartnerAddresses
                .Where(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    (x.Type == AddressType.Shipping || x.Type == AddressType.Common))
                .ToListAsync();

            return MapAddresses(addresses);
        }

        // ────────────────────────────────────────────────────
        // GET ADDRESS BY ID
        // ────────────────────────────────────────────────────
        public async Task<BPAddressResponseDto?> GetAddressByIdAsync(int businessPartnerId, int addressId)
        {
            var companyId = _common.GetCompanyId();
            await ValidateBPAsync(businessPartnerId, companyId);

            var a = await _context.BusinessPartnerAddresses
                .FirstOrDefaultAsync(x =>
                    x.BPAddressId == addressId &&
                    x.BusinessPartnerId == businessPartnerId);

            return a == null ? null : MapAddress(a);
        }

        // ────────────────────────────────────────────────────
        // GET CONTACTS BY BP
        // ────────────────────────────────────────────────────
        public async Task<List<BusinessPartnerContactResponseDto>> GetContactsByBPAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();
            await ValidateBPAsync(businessPartnerId, companyId);

            var contacts = await _context.BusinessPartnerContacts
                .Where(x => x.BusinessPartnerId == businessPartnerId)
                .ToListAsync();

            return MapContacts(contacts);
        }

        // ────────────────────────────────────────────────────
        // GET INVOICE DEFAULTS
        // ────────────────────────────────────────────────────
        public async Task<BPAddressDropdownResponseDto> GetInvoiceDefaultsByBPAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();
            await ValidateBPAsync(businessPartnerId, companyId);

            var allAddresses = await _context.BusinessPartnerAddresses
                .Where(x => x.BusinessPartnerId == businessPartnerId)
                .ToListAsync();

            var allContacts = await _context.BusinessPartnerContacts
                .Where(x => x.BusinessPartnerId == businessPartnerId)
                .ToListAsync();

            var commonAddresses = allAddresses.Where(x => x.Type == AddressType.Common).ToList();
            var billingAddresses = allAddresses.Where(x => x.Type == AddressType.Billing).ToList();
            var shippingAddresses = allAddresses.Where(x => x.Type == AddressType.Shipping).ToList();

            var billList = billingAddresses.Concat(commonAddresses).ToList();
            var shipList = shippingAddresses.Concat(commonAddresses).ToList();

            var defaultBill =
                billingAddresses.FirstOrDefault(x => x.IsDefault)
                ?? commonAddresses.FirstOrDefault(x => x.IsDefault)
                ?? commonAddresses.FirstOrDefault()
                ?? billingAddresses.FirstOrDefault();

            var defaultShip =
                shippingAddresses.FirstOrDefault(x => x.IsDefault)
                ?? commonAddresses.FirstOrDefault(x => x.IsDefault)
                ?? commonAddresses.FirstOrDefault()
                ?? shippingAddresses.FirstOrDefault();

            var defaultContact =
                allContacts.FirstOrDefault(x => x.IsPrimary)
                ?? allContacts.FirstOrDefault();

            return new BPAddressDropdownResponseDto
            {
                DefaultBillAddress = defaultBill != null ? MapAddress(defaultBill) : null,
                DefaultShipAddress = defaultShip != null ? MapAddress(defaultShip) : null,
                DefaultContact = defaultContact != null ? MapContact(defaultContact) : null,
                BillAddresses = MapAddresses(billList),
                ShipAddresses = MapAddresses(shipList),
                Contacts = MapContacts(allContacts)
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ════════════════════════════════════════════════════

        // ── Map BusinessPartner → ResponseDto ────────────────
        private BusinessPartnerResponseDto MapBP(BusinessPartner x)
        {
            return new BusinessPartnerResponseDto
            {
                BusinessPartnerId = x.BusinessPartnerId,
                CompanyId = x.CompanyId,
                BusinessPartnerCode = x.BusinessPartnerCode,
                BusinessPartnerName = x.BusinessPartnerName,
                PrintName = x.PrintName,
                Type = x.Type,
                TypeName = x.Type.ToString(),
                Mobile = x.Mobile,
                Email = x.Email,
                CreditLimit = x.CreditLimit,
                CreditDays = x.CreditDays,
                AccountGroupId = x.AccountGroupId,
                AccountId = x.AccountId,
                IsActive = x.IsActive,
                DefaultPriceType = x.DefaultPriceType,

                BPAddresses = x.BPAddresses?.Select(p => new BusinessPartnerAddressDto
                {
                    BPAddressId = p.BPAddressId,
                    BusinessPartnerId = p.BusinessPartnerId,
                    Type = p.Type,
                    AddressTypeName = p.Type.ToString(),
                    AddressLine1 = p.AddressLine1,
                    AddressLine2 = p.AddressLine2,
                    City = p.City,
                    State = p.State,
                    StateName = p.State.HasValue ? p.State.Value.ToString() : null,
                    StateCode = p.State.HasValue ? ((int)p.State.Value).ToString("D2") : null,
                    Country = p.Country,
                    Pincode = p.Pincode,
                    GSTType = p.GSTType,
                    GSTTypeName = p.GSTType.ToString(),
                    GSTNo = p.GSTNo,
                    IsDefault = p.IsDefault
                }).ToList(),

                BPContacts = x.BPContacts?.Select(p => new BusinessPartnerContactDto
                {
                    BPContactId = p.BPContactId,
                    BusinessPartnerId = p.BusinessPartnerId,
                    Name = p.Name,
                    Mobile = p.Mobile,
                    Email = p.Email,
                    Designation = p.Designation,
                    IsPrimary = p.IsPrimary
                }).ToList()
            };
        }

        // ── Map single Address ───────────────────────────────
        private BPAddressResponseDto MapAddress(BusinessPartnerAddress a)
        {
            return new BPAddressResponseDto
            {
                BPAddressId = a.BPAddressId,
                BusinessPartnerId = a.BusinessPartnerId,
                AddressType = a.Type.ToString(),
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                StateCode = a.State.HasValue ? (int)a.State.Value : null,
                StateName = a.State.HasValue ? a.State.Value.ToString() : null,
                Country = a.Country,
                Pincode = a.Pincode,
                GSTType = a.GSTType.ToString(),
                GSTNo = a.GSTNo,
                IsDefault = a.IsDefault
            };
        }

        // ── Map Address list ─────────────────────────────────
        private List<BPAddressResponseDto> MapAddresses(List<BusinessPartnerAddress> addresses)
            => addresses.Select(MapAddress).ToList();

        // ── Map single Contact ───────────────────────────────
        private BusinessPartnerContactResponseDto MapContact(BusinessPartnerContact c)
        {
            return new BusinessPartnerContactResponseDto
            {
                BPContactId = c.BPContactId,
                BusinessPartnerId = c.BusinessPartnerId,
                Name = c.Name,
                Mobile = c.Mobile,
                Email = c.Email,
                Designation = c.Designation,
                IsPrimary = c.IsPrimary
            };
        }

        // ── Map Contact list ─────────────────────────────────
        private List<BusinessPartnerContactResponseDto> MapContacts(List<BusinessPartnerContact> contacts)
            => contacts.Select(MapContact).ToList();

        // ── Validate BP belongs to company ───────────────────
        private async Task ValidateBPAsync(int businessPartnerId, int companyId)
        {
            var exists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (!exists)
                throw new Exception("Business Partner not found.");
        }
    }
}