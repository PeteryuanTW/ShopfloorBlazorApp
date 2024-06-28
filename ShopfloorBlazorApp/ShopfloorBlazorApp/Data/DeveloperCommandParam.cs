namespace ShopfloorBlazorApp.Data
{
    public class DeveloperCommandParam
    {
        public string hint {  get; set; }
        public int type { get; set; }
        public string typeString { get; set; }
        public string param {  get; set; }

        public DeveloperCommandParam(string hint, int type)
        {
            this.hint = hint;
            this.type = type;
            switch (this.type)
            {
                case 0:
                    typeString = "bool";
                    break;
                case 1:
                    typeString = "Int";
                    break;
                case 2:
                    typeString = "float";
                    break;
                case 3:
                    typeString = "string";
                    break;
                default:
                    typeString = "unknown";
                    break;
            }
            param = String.Empty;
        }
    }
}
