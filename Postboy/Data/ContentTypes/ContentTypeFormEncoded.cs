namespace Postboy.Data.ContentTypes
{
    public class ContentTypeFormEncoded : StoredRequestContentType
    {
        public override string ToString()
        {
            return "X-WWW-Formurl-Encoded";
        }
    }
}
