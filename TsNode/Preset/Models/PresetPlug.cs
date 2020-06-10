using System;
using System.Linq;
using System.Numerics;

namespace TsNode.Preset.Models
{
    public enum IOType
    {
        Input,
        Output,
    }
    
    public class PlugPropertyInfo : PresetNotification , IHasGuid
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public SerializableType PropertyType { get; set; }
        
        public IOType IoType { get; set; }

        public PresetPlug CreatePlug()
        {
            var property = PropertyType.CreateInstance<Property>(typeof(Property<>));
            property.Name = Name;
            property.Value = TypeSerializer.Deserialize(PropertyType.ToSystemType() , Value);
            return new PresetPlug(property)
            {
                Guid = Guid,
            };
        }

        public static PlugPropertyInfo Serialize(PresetPlug plug , IOType io)
        {
            var info = new PlugPropertyInfo();
            info.Guid = plug.Guid;
            info.PropertyType = plug.Property.GetGenericType().ToSerializableType();
            info.Value = plug.Property.Value.Serialize();
            info.Name = plug.Property.Name;
            info.IoType = io;
            return info;
        }
    }
    
    public class PresetPlug : PresetNotification , IHasGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Property Property { get; private set; }

        public PresetPlug(Property property)
        {
            Property = property;
        }
    }
}