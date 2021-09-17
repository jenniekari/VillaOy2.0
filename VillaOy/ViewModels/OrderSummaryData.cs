namespace VillaOy.ViewModels
{
    using System;
    public class OrderSummaryData
    {
        public int TilausID { get; set; }
        public int AsiakasID { get; set; }
        public string Toimitusosoite { get; set; }
        public string Postinumero { get; set; }
        public Nullable<System.DateTime> Tilauspvm { get; set; }
        public Nullable<System.DateTime> Toimituspvm { get; set; }
        public int TilausriviID { get; set; }
        public int TuoteID { get; set; }
        public Nullable<int> Maara { get; set; }
        public Nullable<float> Ahinta { get; set; } 

    }
}