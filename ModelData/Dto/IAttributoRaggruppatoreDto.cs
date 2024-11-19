using ModelData.Model;

namespace ModelData.Dto
{
    public interface IAttributoRaggruppatoreDto : ICloneable
    {
        object ICloneable.Clone()
        {
            return Clone();
        }
        Guid ProgettoId { get; set; }
        Guid AttributoId { get; set; }
        string? Codice { get; set; }
        string? CodiceGruppo { get; set; }
        string? Valore { get; set; }
        string? Descrizione { get; set; }
        string? Tooltip { get; set; }
        int? SequenceNumber { get; set; }
        bool Expanded { get; set; }
        bool Selected { get; set; }
        bool IsRemoved { get; set; }
        string? FiltroCondizione { get; set; }
        List<Guid>? EntitiesId { get; set; }
        List<string>? ValoriUnivociOrdered { get; set; }
        List<IAttributoRaggruppatoreDto>? SottoGruppo { get; set; }
    }
}
