namespace RelationshipService.Application.Models.Profile.Requests;

public class SyncAnswersRequest
{
    public List<UpsertAnswerRequest> Answers { get; set; }
}
