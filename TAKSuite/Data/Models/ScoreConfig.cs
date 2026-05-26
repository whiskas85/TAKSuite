using System.Text.Json;

namespace TAKSuite.Data.Models
{
    public class FaseEDef
    {
        public string Descrizione { get; set; } = "";
        public int MaxVal { get; set; }  // 0 = nessuna validazione
    }

    public class ScoreConfig : IGuidModel
    {
        public Guid Id { get; set; }
        public Guid? TaskEntityId { get; set; }           // not null = override obiettivo
        public TaskEntity? Task { get; set; }
        public Guid? MissionTAKSuiteId { get; set; }     // not null, TaskEntityId null = config missione; entrambi null = DEFAULT globale

        // Valori massimi per campo (0 = nessuna validazione)
        public int MaxBivacco { get; set; }
        public int MaxAiutoCartografico { get; set; }
        public int MaxOperatoreNonDichiarato { get; set; }
        public int MaxMarcaturaAsgAssente { get; set; }
        public int MaxFasciaNonEsposta { get; set; }
        public int MaxOperatoreSqualificato { get; set; }
        public int MaxInterferenzaArbitrale { get; set; }
        public int MaxComportamentoAntisportivo { get; set; }
        public int MaxAsgOverJoule { get; set; }
        public int MaxSaccoRifiuti { get; set; }
        public int MaxDifensoriEliminati { get; set; }
        public int MaxRibelliColpiti { get; set; }
        public int MaxCiviliColpiti { get; set; }

        public bool MostraFasiENulle { get; set; }

        // Definizioni Fasi E: JSON FaseEDef[7]
        public string FasiEJson { get; set; } = "[]";

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public FaseEDef[] FasiEDefs
        {
            get
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<FaseEDef[]>(FasiEJson);
                    var r = new FaseEDef[7];
                    for (int i = 0; i < 7; i++)
                        r[i] = arr != null && i < arr.Length && arr[i] != null ? arr[i] : new FaseEDef();
                    return r;
                }
                catch { return Enumerable.Range(0, 7).Select(_ => new FaseEDef()).ToArray(); }
            }
            set => FasiEJson = JsonSerializer.Serialize(value ?? new FaseEDef[7]);
        }
    }
}
