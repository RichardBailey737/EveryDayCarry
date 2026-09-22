using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur.Core.Utilities.Triggers
{
    /// <summary>
    /// Generated from the DCS_TRIGGER_EVENT_TYPE table
    /// </summary>
    /// <remarks>
    /// Generation query:
    /// SELECT REPLACE(dtet.EVENT_TYPE, ' ', '') + ' = ' + CAST(dtet.EVENT_TYPE_ID AS VARCHAR) + ',' AS GENERATED FROM DCS_TRIGGER_EVENT_TYPES dtet 
    /// </remarks>
    public enum TriggerTypes
    {
        FormSave = 1,
        FormGridSave = 2,
        CreateContentRecord = 3,
        ReviseContentRecord = 4,
        ReplicateContentRecord = 5,
        LinkCreateLoad = 6,
        LinkCreateComplete = 7,
        CheckOut = 8,
        FormPageLoad = 9,
        FormAboutLoad = 10,
        FormCheckOut = 11,
        FormUpdateLayoutLoad = 12,
        PeriodicReviewLoad = 13,
        ApproveLoad = 14,
        ApprovalSig = 15,
        ApprovalCompleted = 16,
        ReviewLoad = 17,
        ReviewSig = 18,
        ReviewCompleted = 19,
        AIComplete = 20,
        DocTrainingMarkasReadLoad = 21,
        DocTrainingMarkasReadSig = 22,
        IncidentLoad = 23,
        IncidentSave = 24,
        IncidentOpen = 25,
        IncidentClose = 26,
        ContentExplorerLoad = 27,
        HomeLoad = 28,
        SubmitApprovalLoad = 29,
        SubmitApprovalComplete = 30,
        SubmitReviewLoad = 31,
        SubmitReviewComplete = 32,
        FormRelatedChildLoad = 33,
        FormRelatedChildLinked = 34,
        FormRelatedParentLoad = 35,
        FormRelatedParentLinked = 36,
        CheckIn = 37,
        PeriodicReviewCompleted = 38,
        SysAdminContentTypeOwnerAssociation = 39,
        SysAdminContentTypeOwnerDeassociation = 40,
        SysAdminContentTypeSave = 41,
        SysAdminContentTypeSiteAssociation = 42,
        SysAdminContentTypeSiteDeassociation = 43,
        SysAdminDeativateUser = 44,
        SysAdminGroupDeactivate = 45,
        SysAdminGroupReactivate = 46,
        SysAdminGroupSave = 47,
        SysAdminServiceSummary = 48,
        SysAdminUserGroupAssociation = 49,
        SysAdminUserGroupDeassociation = 50,
        SysAdminUserJTAssociation = 51,
        SysAdminUserJTDeassociation = 52,
        SysAdminUserReactivate = 53,
        SysAdminUserSave = 54,
        SysAdminUserSiteAssociation = 55,
        SysAdminUserSiteDeassociation = 56,
        SysAdminUserSubordinateAssociation = 57,
        SysAdminUserSubordinateDeassociation = 58,
        SysAdminUserSupervisorAssociation = 59,
        SysAdminUserSupervisorDeassociation = 60,
        AuthenticateComplete = 61,
        EnsurUpdateFired = 62,
        StatusChange = 63

    }


    /// <summary>
    /// Generated from DCS_TRIGGER_EVENT_CALL_TYPES 
    /// </summary>
    public enum TriggerCallTypes
    {
        StoredProcedure=1,
        CSScript=2,
        Plugin=3,
        APICall=4        

    }
}
