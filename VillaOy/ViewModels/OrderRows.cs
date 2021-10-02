namespace VillaOy.ViewModels
{
    using System;
    using System.Drawing;
    public class OrderRows
    {
        public int TilausriviID { get; set; }
        public int TuoteID { get; set; }
        public string Toimitusosoite { get; set; }
        public string Postinumero { get; set; }
        public Nullable<int> Maara { get; set; }
        public Nullable<decimal> Ahinta { get; set; }

    }
}