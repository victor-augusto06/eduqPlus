namespace EduqPlus.API.DTOs {
    public class PagedResultDTO<T> {
        public IEnumerable<T> Itens { get; set; } = new List<T>();
        public int TotalItens { get; set; }
        public int PaginaAtual { get; set; }
        public int TamanhoPagina { get; set; }
        public int TotalPaginas => (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
    }
}