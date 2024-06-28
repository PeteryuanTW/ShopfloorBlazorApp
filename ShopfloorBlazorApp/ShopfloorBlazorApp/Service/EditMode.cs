namespace ShopfloorBlazorApp.Service
{
    public class EditMode
    {
        //0:new 1:edit 2:delete
        private int _mode = 0;
        public int mode => _mode;
        private bool _pkeditable = true;
        public bool pkeditable => _pkeditable;
        private string _text = "New";
        public string text => _text;

        public EditMode()
        {
            _mode = 0;
            _pkeditable = true;
            _text = "New";
        }

        public void New()
        {
            _mode = 0;
            _pkeditable = true;
            _text = "New";
        }
        public void Edit()
        {
            _mode = 1;
            _pkeditable = false;
            _text = "Edit";
        }
        public void Delete()
        {
            _mode = 2;
            _pkeditable = false;
            _text = "Delete";
        }
    }
}
