using System.Text.Json;

namespace TAKSuite.Data.Models
{
    public class TaskScoreEntry : IGuidModel
    {
        public Guid Id { get; set; }
        public Guid TaskEntityId { get; set; }
        public TaskEntity? Task { get; set; }

        public string IdentificativoPattuglia { get; set; } = "";
        public string? Tipologia { get; set; }
        public int? DurataMinuti { get; set; }
        public string? InizioObj { get; set; }  // "HH:mm"
        public string? FineObj { get; set; }    // "HH:mm"
        public bool FuoriFinestra { get; set; }
        public bool PunteggioMinimo { get; set; }

        // Penalità (conteggi)
        public int Bivacco { get; set; }
        public int AiutoCartografico { get; set; }
        public int OperatoreNonDichiarato { get; set; }
        public int MarcaturaAsgAssente { get; set; }
        public int FasciaNonEsposta { get; set; }
        public int OperatoreSqualificato { get; set; }
        public int InterferenzaArbitrale { get; set; }
        public int ComportamentoAntisportivo { get; set; }
        public int AsgOverJoule { get; set; }
        public int SaccoRifiuti { get; set; }  // neutro

        // Punteggio positivo
        public int DifensoriEliminati { get; set; }
        public int RibelliColpiti { get; set; }
        public int CiviliColpiti { get; set; }

        // Fasi E: JSON bool?[7] — true=Sì, false=No, null=vuoto/non considerare
        public string FasiEJson { get; set; } = "[]";
        public bool MostraFasiENulle { get; set; }

        // Note e contestazioni
        public string? NotePattugliaIncursori { get; set; }
        public string? NoteArbitro { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool?[] FasiE
        {
            get
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<bool?[]>(FasiEJson);
                    var r = new bool?[7];
                    if (arr != null)
                        for (int i = 0; i < Math.Min(arr.Length, 7); i++) r[i] = arr[i];
                    return r;
                }
                catch { return new bool?[7]; }
            }
            set => FasiEJson = JsonSerializer.Serialize(value ?? new bool?[7]);
        }
    }
}
