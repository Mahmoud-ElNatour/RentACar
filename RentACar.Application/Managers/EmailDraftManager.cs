using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;
using RentACar.Core.Repositories;

namespace RentACar.Application.Managers
{
    public class EmailDraftManager
    {
        private readonly IEmailDraftRepository _draftRepository;

        public EmailDraftManager(IEmailDraftRepository draftRepository)
        {
            _draftRepository = draftRepository;
        }

        public async Task<List<EmailDraftDto>> GetDraftsByUserAsync(string userId)
        {
            var drafts = await _draftRepository.GetDraftsByUserIdAsync(userId);
            return drafts.Select(d => new EmailDraftDto
            {
                Id = d.Id,
                Subject = d.Subject,
                Body = d.Body,
                RecipientsRaw = d.RecipientsRaw,
                SelectedDistributionListIdsRaw = d.SelectedDistributionListIdsRaw,
                LastUpdated = d.UpdatedAt ?? d.CreatedAt
            }).ToList();
        }

        public async Task<int> SaveDraftAsync(SaveDraftRequestDto draftDto, string userId)
        {
            EmailDraft draft = null;

            if (draftDto.Id > 0)
            {
                draft = await _draftRepository.GetDraftByIdAndUserIdAsync(draftDto.Id, userId);
            }

            if (draft == null)
            {
                draft = new EmailDraft
                {
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            draft.Subject = draftDto.Subject;
            draft.Body = draftDto.Body;
            draft.RecipientsRaw = draftDto.RecipientsRaw;
            draft.SelectedDistributionListIdsRaw = draftDto.SelectedDistributionListIdsRaw;
            draft.UpdatedAt = DateTime.UtcNow;

            if (draft.Id == 0)
            {
                await _draftRepository.AddAsync(draft);
            }
            else
            {
                await _draftRepository.UpdateAsync(draft);
            }

            return draft.Id;
        }

        public async Task<EmailDraftDto> GetDraftAsync(int id, string userId)
        {
            var draft = await _draftRepository.GetDraftByIdAndUserIdAsync(id, userId);
            if (draft == null) return null;

            return new EmailDraftDto
            {
                Id = draft.Id,
                Subject = draft.Subject,
                Body = draft.Body,
                RecipientsRaw = draft.RecipientsRaw,
                SelectedDistributionListIdsRaw = draft.SelectedDistributionListIdsRaw,
                LastUpdated = draft.UpdatedAt ?? draft.CreatedAt
            };
        }

        public async Task DeleteDraftAsync(int id, string userId)
        {
            var draft = await _draftRepository.GetDraftByIdAndUserIdAsync(id, userId);
            if (draft != null)
            {
                await _draftRepository.DeleteAsync(draft);
            }
        }
    }
}
