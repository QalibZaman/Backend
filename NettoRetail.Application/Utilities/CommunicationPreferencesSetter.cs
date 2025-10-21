using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Utilities;

public static class CommunicationPreferencesSetter
{
    public static CommunicationPreferenceEntity SetCommunicationPreferences(AccountEntity user)
    {
        CommunicationPreferenceEntity preferences = new CommunicationPreferenceEntity(user.Id);
        if (user.PhoneNumber == null)
        {
            preferences.OrderUpdateSms = false;
            preferences.CommunicationPreferenceSms = false;
            preferences.ShipmentUpdateSms = false;
            preferences.PromotionSms = false;
        }

        return preferences;
    }
}
