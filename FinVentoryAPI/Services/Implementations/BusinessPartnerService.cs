using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.BusinessPartnerDTOs;
using FinVentoryAPI.DTOs.ItemDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Reflection;
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

        // Fix CreateAsync — create Account exactly like AccountService.CreateAsync does
        // Replace only the account creation block in BusinessPartnerService.cs

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

            // ── Create ledger account — same fields as AccountService.CreateAsync ──
            var account = new Account
            {
                AccountName = dto.BPName,
                AccountCode = dto.BPCode,
                AccountGroupId = dto.AccountGroupId,
                AccountType = AccountType.General,
                BookType = null,          // not required — same as AccountService
                BookSubType = null,          // not required — same as AccountService
                CompanyId = companyId,
                CreatedBy = _common.GetUserId(),
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();  // AccountId generated here

            // ── Create Business Partner ────────────────────────────────────────────
            var bp = new BusinessPartner
            {
                CompanyId = companyId,
                BusinessPartnerCode = dto.BPCode,
                BusinessPartnerName = dto.BPName,
                PrintName = dto.PrintName,
                Type = dto.BPType,
                Mobile = dto.Mobile,
                Email = dto.Email,
                CreditLimit = dto.CreditLimit,
                CreditDays = dto.CreditDays,
                AccountGroupId = dto.AccountGroupId,
                AccountId = account.AccountId,  // link to newly created account
                CreatedBy = _common.GetUserId(),
                DefaultPriceType = dto.DefaultPriceType
                
            };

            _context.BusinessPartners.Add(bp);
            await _context.SaveChangesAsync();

            // ── Addresses ─────────────────────────────────────────────────────────
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

            // ── Contacts ──────────────────────────────────────────────────────────
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

        //public async Task<BusinessPartnerResponseDto> CreateAsync(CreateBusinessPartnerDto dto)
        //{
        //    var companyId = _common.GetCompanyId();

        //    var duplicate = await _context.BusinessPartners
        //        .AnyAsync(x =>
        //            x.CompanyId == companyId &&
        //            x.BusinessPartnerName.ToLower() == dto.BPName.ToLower() &&
        //            !x.IsDeleted);

        //    if (duplicate)
        //        throw new Exception("Business Partner already exists.");

        //    var account = new Account
        //    {
        //        AccountName = dto.BPName,           // or dto.PrintName, your choice
        //        AccountCode = dto.BPCode,
        //        AccountGroupId = dto.AccountGroup,  // make sure this is an int GroupId
        //        AccountType = AccountType.General,  
        //        CompanyId = companyId,
        //        CreatedBy = _common.GetUserId()
        //    };

        //    _context.Accounts.Add(account);
        //    await _context.SaveChangesAsync();      // ← AccountId is now generated

        //    var bp = new BusinessPartner
        //    {
        //        CompanyId = companyId,
        //        BusinessPartnerCode = dto.BPCode,
        //        BusinessPartnerName = dto.BPName,
        //        PrintName = dto.PrintName,
        //        Type = dto.BPType,
        //        Mobile = dto.Mobile,
        //        Email = dto.Email,
        //        CreditLimit = dto.CreditLimit,
        //        CreditDays = dto.CreditDays,
        //        AccountGroup = dto.AccountGroup,
        //        AccountId = account.AccountId,
        //        CreatedBy = _common.GetUserId()
        //    };

        //    _context.BusinessPartners.Add(bp);
        //    await _context.SaveChangesAsync();

        //    // 💰 Addresses
        //    if (dto.BPAddresses != null && dto.BPAddresses.Any())
        //    {
        //        var addresses = dto.BPAddresses.Select(p => new BusinessPartnerAddress
        //        {
        //            BusinessPartnerId = bp.BusinessPartnerId,
        //            Type = p.Type,
        //            AddressLine1 = p.AddressLine1,
        //            AddressLine2 = p.AddressLine2,
        //            City = p.City,
        //            State = p.State,
        //            Country = p.Country,
        //            Pincode = p.Pincode,
        //            GSTType = p.GSTType,
        //            GSTNo = p.GSTNo,
        //            IsDefault = p.IsDefault,
        //        }).ToList();

        //        _context.BusinessPartnerAddresses.AddRange(addresses);
        //        await _context.SaveChangesAsync();
        //    }

        //    // 💰 Contacts
        //    if (dto.BPContacts != null && dto.BPContacts.Any())
        //    {
        //        var contacts = dto.BPContacts.Select(p => new BusinessPartnerContact
        //        {
        //            BusinessPartnerId = bp.BusinessPartnerId,
        //            Name = p.Name,
        //            Mobile = p.Mobile,
        //            Email = p.Email,
        //            Designation = p.Designation,
        //            IsPrimary = p.IsPrimary,
        //        }).ToList();

        //        _context.BusinessPartnerContacts.AddRange(contacts);
        //        await _context.SaveChangesAsync();
        //    }

        //    return await GetByIdAsync(bp.BusinessPartnerId);
        //}

        // ✅ UPDATE
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
                throw new Exception("Business Partners same name already exists.");

            // Update
            bp.BusinessPartnerCode = dto.BPCode;
            bp.BusinessPartnerName = dto.BPName;
            bp.PrintName = dto.PrintName;
            bp.Type = dto.BPType;
            bp.Mobile = dto.Mobile;
            bp.Email = dto.Email;
            bp.CreditLimit = dto.CreditLimit;
            bp.CreditDays = dto.CreditDays;
            bp.AccountGroupId = dto.AccountGroupId;
            bp.AccountId = dto.AccountId;
            bp.IsActive = dto.IsActive;
            bp.ModifiedBy = _common.GetUserId();
            bp.ModifiedDate = DateTime.UtcNow;
            bp.DefaultPriceType = dto.DefaultPriceType;


            // 🔥 Replace Addresses
            if (dto.BPAddresses != null)
            {
                _context.BusinessPartnerAddresses.RemoveRange(bp.BPAddresses);

                var newAddress = dto.BPAddresses.Select(p => new BusinessPartnerAddress
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

                await _context.BusinessPartnerAddresses.AddRangeAsync(newAddress);
            }

            // 🔥 Replace Contacts
            if (dto.BPContacts != null)
            {
                _context.BusinessPartnerContacts.RemoveRange(bp.BPContacts);

                var newContact = dto.BPContacts.Select(p => new BusinessPartnerContact
                {
                    BusinessPartnerId = bp.BusinessPartnerId,
                    Name = p.Name,
                    Mobile = p.Mobile,
                    Email = p.Email,
                    Designation = p.Designation,
                    IsPrimary = p.IsPrimary,
                });

                await _context.BusinessPartnerContacts.AddRangeAsync(newContact);
            }

            await _context.SaveChangesAsync();
            return true;
        }

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

            return new BusinessPartnerResponseDto
            {

                BusinessPartnerId = id,
                Type = bp.Type,
                BusinessPartnerCode = bp.BusinessPartnerCode,
                BusinessPartnerName = bp.BusinessPartnerName,
                PrintName = bp.PrintName,
                Mobile = bp.Mobile,
                Email = bp.Email,
                CreditLimit = bp.CreditLimit,
                CreditDays = bp.CreditDays,
                AccountGroupId = bp.AccountGroupId,
                AccountId = bp.AccountId,
                DefaultPriceType = bp.DefaultPriceType,


                // ── Address ────────────────────────────────────────────
                BPAddresses = bp.BPAddresses?.Select(p => new BusinessPartnerAddressDto
                {
                    Type = p.Type,
                    AddressLine1 = p.AddressLine1,
                    AddressLine2 = p.AddressLine2,
                    City = p.City,
                    State = p.State,
                    Country = p.Country,
                    Pincode = p.Pincode,
                    GSTType = p.GSTType,
                    GSTNo = p.GSTNo
                }).ToList(),

                // ── Contacts ────────────────────────────────────────────
                BPContacts = bp.BPContacts?.Select(p => new BusinessPartnerContactDto
                {
                    Name = p.Name,
                    Mobile = p.Mobile,
                    Email = p.Email,
                    Designation = p.Designation,
                    IsPrimary = p.IsPrimary
                }).ToList()

            };
        }

        public async Task<List<BusinessPartnerResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var businessPartners = await _context.BusinessPartners
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .ToListAsync();

            return businessPartners.Select(x => new BusinessPartnerResponseDto
            {
              BusinessPartnerId = x.BusinessPartnerId,
              Type = x.Type,
              BusinessPartnerCode = x.BusinessPartnerCode,
              BusinessPartnerName = x.BusinessPartnerName,
              PrintName = x.PrintName,
              Mobile = x.Mobile,
              Email = x.Email,
              CreditLimit = x.CreditLimit,                
              CreditDays = x.CreditDays,
              AccountGroupId = x.AccountGroupId,
              AccountId = x.AccountId,
              DefaultPriceType = x.DefaultPriceType

            }).ToList();
        }

        // ✅ PAGED
        public async Task<PagedResponseDto<BusinessPartnerResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.BusinessPartners
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.accountGroup)
                .AsQueryable();

            // 🔍 SEARCH
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();

                query = query.Where(x =>
                    x.BusinessPartnerName.ToLower().Contains(search) ||
                    (x.BusinessPartnerName ?? "").ToLower().Contains(search));

            }

            // 🎯 FILTERS
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

            // 🔽 SORTING
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

            // 📊 TOTAL COUNT
            var totalRecords = await query.CountAsync();

            // 📄 DATA
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new BusinessPartnerResponseDto
                {
                    BusinessPartnerId = x.BusinessPartnerId,
                    BusinessPartnerName = x.BusinessPartnerName,
                    PrintName = x.PrintName,
                    Type = x.Type,
                    Mobile = x.Mobile,
                    Email = x.Email,
                    CreditLimit = x.CreditLimit,
                    CreditDays = x.CreditDays,
                    AccountGroupId = x.AccountGroupId,
                    AccountId = x.AccountId,
                    IsActive = x.IsActive

                })
                .ToListAsync();

            return new PagedResponseDto<BusinessPartnerResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data
            };
        }
    }
}
