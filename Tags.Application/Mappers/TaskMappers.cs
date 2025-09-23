using Tags.Application.Dtos;

namespace Tags.Application.Mappers;

public static class TaskMappers
{
    public static TagDto ToDto(this Domain.Entities.Tags tag)
    {
        return new TagDto(
            tag.Id,
            tag.Nome,
            tag.Cor,
            tag.Descricao
        );
    }
}