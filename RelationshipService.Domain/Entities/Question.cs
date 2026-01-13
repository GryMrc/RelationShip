using RelationshipService.Domain.Base;

namespace RelationshipService.Domain.Entities;

public class Question : Entity<int>
{
    public string Text { get; private set; }

    public List<QuestionAnswer> QuestionAnswers { get; set; }

    /*
    Smokking, Education, Excersize, Driking, Pets, Loking For, Kids, Politics, Religion, Work, WhereIsLive, Ayak numarası, Patlak mı kız oğlu kız mı, 
    En büyük korkun, Sevişmekten en çok hoşlandığın yer, En sevdiğin pozisyon
    */
}