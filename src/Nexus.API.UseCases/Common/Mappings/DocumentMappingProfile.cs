using AutoMapper;
using Nexus.API.Core.Aggregates.DocumentAggregate;
using Nexus.API.UseCases.Common.DTOs;

namespace Nexus.API.UseCases.Common.Mappings;

/// <summary>
/// AutoMapper profile for Document mappings
/// </summary>
public class DocumentMappingProfile : Profile
{
    public DocumentMappingProfile()
    {
        // Document -> DocumentDto
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Value))
            .ForMember(dest => dest.ContentRichText, opt => opt.MapFrom(src => src.Content.RichText))
            .ForMember(dest => dest.ContentPlainText, opt => opt.MapFrom(src => src.Content.PlainText))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.Content.WordCount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.Versions, opt => opt.MapFrom(src => src.Versions));

        // Document -> DocumentSummaryDto
        CreateMap<Document, DocumentSummaryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title.Value))
            .ForMember(dest => dest.Excerpt, opt => opt.MapFrom(src => CreateExcerpt(src.Content.PlainText)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.WordCount, opt => opt.MapFrom(src => src.Content.WordCount))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Name).ToList()));

        // Tag -> TagDto
        CreateMap<Tag, TagDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // DocumentVersion -> DocumentVersionDto
        CreateMap<DocumentVersion, DocumentVersionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }

    private static string CreateExcerpt(string plainText, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        if (plainText.Length <= maxLength)
            return plainText;

        return plainText.Substring(0, maxLength) + "...";
    }
}
