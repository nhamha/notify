using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class Event2016
    {

    }
    public class SpinModel
    {
        public long SpinID { get; set; }
        public int LIXI { get; set; }
        public long PrizeValue { get; set; }
        public int vatphamType { get; set; }
        public int VatphamGroupID { get; set; }
        public string TenVatpham { get; set; }
        public long BitValue { get; set; }
        public long BacValue { get; set; }
        public long ResponseStatus { get; set; }
        public bool IsGot { get; set; }
        public int ValType { get; set; }
    }
}
