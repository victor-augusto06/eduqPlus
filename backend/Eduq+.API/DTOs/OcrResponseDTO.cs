using System.Text.Json.Serialization;

namespace EduqPlus.API.DTOs {

    public class OcrResultDTO {

        [JsonPropertyName("nome_arquivo")]
        public string NomeArquivo { get; set; }

        [JsonPropertyName("texto")]
        public string Texto { get; set; }
    }


    public class OcrResponseDTO {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("resultados")]
        public List<OcrResultDTO> Resultados { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}