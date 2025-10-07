using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Domain.Entities
{
    public class CommunicationPreferenceEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public bool CommunicationPreferenceEmail { get; set; } = true;
        public bool CommunicationPreferenceSms { get; set; } = true;
        public bool CommunicationPreferencePush { get; set; } = true;

        public bool OrderUpdateEmail { get; set; } = true;
        public bool OrderUpdateSms { get; set; } = true;
        public bool OrderUpdatePush { get; set; } = true;

        public bool PromotionEmail { get; set; } = true;
        public bool PromotionSms { get; set; } = true;
        public bool PromotionPush { get; set; } = true;

        public bool ShipmentUpdateEmail { get; set; } = true;
        public bool ShipmentUpdateSms { get; set; } = true;
        public bool ShipmentUpdatePush { get; set; } = true;

        public AccountEntity? User { get; set; }

        public CommunicationPreferenceEntity(Guid userId)
        {
            UserId = userId;
        }

        private CommunicationPreferenceEntity() { }

    }
}
