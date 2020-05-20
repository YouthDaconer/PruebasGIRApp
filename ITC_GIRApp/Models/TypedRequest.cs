namespace ITC_GIRApp.Models
{
    public class TypedRequest<T>
    {
        public int Id { get; set; }

        public string Msg { get; set; }

        public bool License { get; set; }

        public string Type { get; set; }

        public string Redirect { get; set; }

        public T Data { get; set; }

        public string Menu { get; set; }

        public string Label { get; set; }

        public string Json { get; set; }
    }
}
