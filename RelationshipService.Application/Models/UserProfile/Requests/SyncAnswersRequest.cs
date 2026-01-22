namespace RelationshipService.Application.Models.UserProfile.Requests;

public class SyncAnswersRequest
{
    public List<UpsertAnswerRequest> Answers { get; set; }
}
