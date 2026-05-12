using EduqPlus.API.Enums;

public class DenunciaCreateDTO {
    public Guid UsuarioId { get; set; }
    public Guid CursoId { get; set; }
    public string RelatoDetalhado { get; set; } = null!;
    public ETipoDenuncia Categoria { get; set; }
}

public class DenunciaResponseDTO {
    public Guid Id { get; set; }
    public Guid CursoId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime Data { get; set; }
    public ETipoDenuncia Categoria { get; set; }
    public string RelatoDetalhado { get; set; } = null!;
    public EStatusDenuncia Status { get; set; }
}

public class DenunciaUpdateDTO {
    public ETipoDenuncia Categoria { get; set; }
    public string RelatoDetalhado { get; set; } = null!;

}

public class DenunciaUpdateAdminDTO: DenunciaUpdateDTO {
    public EStatusDenuncia Status { get; set; }
}