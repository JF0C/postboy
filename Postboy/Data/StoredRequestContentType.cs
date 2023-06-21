using Postboy.Data.ContentTypes;

namespace Postboy.Data
{
    public class StoredRequestContentType
    {
        public static StoredRequestContentType None => new ContentTypeNone();
        public static StoredRequestContentType Json => new ContentTypeJson();
        public static StoredRequestContentType FormEncoded => new ContentTypeFormEncoded();
        public static string Serialize(StoredRequestContentType contentType)
        {
            if (contentType is ContentTypeJson)
            {
                return Json.ToString()!;
            }
            if (contentType is ContentTypeFormEncoded)
            {
                return FormEncoded.ToString()!;
            }
            return None.ToString()!;
        }

        public static StoredRequestContentType Deserialize(string contentType)
        {
            if (contentType == Json.ToString())
            {
                return Json;
            }
            if (contentType == FormEncoded.ToString())
            {
                return FormEncoded;
            }
            return None;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StoredRequestContentType req)
            {
                return req.ToString() == ToString();
            }
            return base.Equals(obj);
        }
    }
}
