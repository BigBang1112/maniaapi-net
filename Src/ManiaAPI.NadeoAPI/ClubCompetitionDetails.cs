using System.Collections.Immutable;

namespace ManiaAPI.NadeoAPI;

public sealed record ClubCompetitionDetails(ClubCompetitionInfo ClubCompetition,
                                            ImmutableList<ClubCompetitionRoundSummary> Rounds,
                                            CompetitionParticipant? Participant,
                                            string? CurrentMatchLiveId,
                                            ClubCompetitionQualifierChallenge? CurrentQualifierChallenge,
                                            bool IsRegistrationOngoing);
