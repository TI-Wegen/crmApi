namespace Conversations.Domain.ValueObjects;

public record Avaliacao
{
    public int Nota { get; private init; }
    public string? Comentario { get; private init; }

    // Construtor privado para garantir a criação válida através do método de fábrica
    private Avaliacao(int nota, string? comentario)
    {
        if (nota < 1 || nota > 5)
            throw new ArgumentOutOfRangeException(nameof(nota), "A nota deve ser entre 1 e 5.");
        Nota = nota;
        Comentario = comentario;
    }

    public static Avaliacao Criar(int nota, string? comentario = null)
    {
        return new Avaliacao(nota, comentario);
    }
}