
namespace NettoRetail.Application.Models.Request.Staff
{
    public class UpdateCommunicationPreferencesRequest
    {
        public required bool CommunicationPreferenceEmail { get; set; }
        public required bool CommunicationPreferenceSms { get; set; }
        public required bool CommunicationPreferencePush { get; set; }

        public required bool OrderUpdateEmail { get; set; }
        public required bool OrderUpdateSms { get; set; }
        public required bool OrderUpdatePush { get; set; }

        public required bool PromotionEmail { get; set; }
        public required bool PromotionSms { get; set; }
        public required bool PromotionPush { get; set; }

        public required bool ShipmentUpdateEmail { get; set; }
        public required bool ShipmentUpdateSms { get; set; }
        public required bool ShipmentUpdatePush { get; set; }
    }
}
