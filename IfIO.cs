using System;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;

namespace YamlTest
{
    public interface IVector2<T>
    {
        T X { get; set; }
        T Y { get; set; }
    }

    public interface IVector3<T>
    {
        T X { get; set; }
        T Y { get; set; }
        T Z { get; set; }
    }

    public class Vector2<T> : IVector2<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        public Vector2()
        {

        }

        public Vector2(T x,T y)
        {
            X = x;
            Y = y;
        }
    }

    public class Vector3<T> : IVector2<T> , IVector3<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        public Vector3() 
        {

        }

        public Vector3(T x, T y , T z) 
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Vector2f : Vector2<float>
    {
        public static Vector2f Zero { get; } = new Vector2f(0, 0);
        public static Vector2f One  { get; } = new Vector2f(1, 1);

        public Vector2f():base()
        {

        }

        public Vector2f(float x, float y):base(x,y)
        {

        }

    }

    public class Vector3f : Vector3<float>
    {
        public static Vector3f Zero { get; } = new Vector3f(0, 0, 1);
        public static Vector3f One  { get; } = new Vector3f(1, 1, 1);

        public Vector3f() : base()
        {

        }

        public Vector3f(float x, float y , float z) : base(x, y,z)
        {

        }
    }


    public interface IGuid
    {
        Guid Guid { get; set; }
    }

    public class Node : IGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Vector2f Position { get; set; } = Vector2f.Zero;
    }

    public enum PlugIO
    {
        Unknown,
        Input,
        Output
    }

    public class Plug : IGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid NodeGuid { get; set; } = Guid.Empty;
        public PlugIO PlugIO { get; set; } = PlugIO.Unknown;
    }

    public class Connection : IGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid SourceGuid { get; set; } = Guid.Empty;
        public Guid DestGuid { get; set; } = Guid.Empty;
    }

    public class Resource : IGuid
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public IGuid[] RawData { get; set; }
    }

    public class Test
    {
        public void Entry()
        {

            var deSerializer = build_deserializer(Assembly.GetEntryAssembly().GetTypes());
            var serialize = deSerializer.Deserialize<Resource>(File.ReadAllText("d:\\test.yml"));

            var serializer = build_serializer(Assembly.GetEntryAssembly().GetTypes());
            var strWriter = new StringWriter();
            serializer.Serialize(strWriter, build_resources());

            File.WriteAllText("d:\\test.yml", strWriter.ToString());
        }

        public IDeserializer build_deserializer(params Type[] types)
        {
            var builder = new DeserializerBuilder();

            foreach (var type in types)
            {
                builder = builder.WithTagMapping($"!{type.FullName}", type);
            }

            return builder
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public ISerializer build_serializer(params Type[] types)
        {
            var builder = new SerializerBuilder();

            foreach (var type in types)
            {
                builder = builder.WithTagMapping($"!{type.FullName}", type);
            }

            return builder
                .EmitDefaults()
                //.EnsureRoundtrip()
                .Build();
        }

        public Resource build_resources()
        {
            return new Resource()
            {
                RawData = new IGuid[]
                {
                    new Node(),
                    new Plug()
                    {
                        PlugIO = PlugIO.Input,
                    },
                    new Plug()
                    {
                        PlugIO = PlugIO.Output,
                    },
                    new Connection(),
                }
            };

        }
    }
}
