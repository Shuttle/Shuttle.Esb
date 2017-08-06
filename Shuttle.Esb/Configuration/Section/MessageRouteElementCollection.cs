using System.Configuration;

namespace Shuttle.Esb
{
    [ConfigurationCollection(typeof(MessageRouteElement), AddItemName = "messageRoute",
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class MessageRouteElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MessageRouteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MessageRouteElement) element).Uri;
        }
    }
}