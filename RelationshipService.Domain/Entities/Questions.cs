namespace RelationshipService.Domain.Entities;

public class Question
{
    public int Id { get; private set; }

    public string Text { get; private set; }

    public List<QuestionAnswer> QuestionAnswers { get; set; }

    public Question(int id, string text)
    {
        Id = id;
        Text = text;
    }

    /*
    Smokking, Education, Excersize, Driking, Pets, Loking For, Kids, Politics, Religion, Work, WhereIsLive, Ayak numarası, Patlak mı kız oğlu kız mı, 
    En büyük korkun, Sevişmekten en çok hoşlandığın yer, En sevdiğin pozisyon
    */
}