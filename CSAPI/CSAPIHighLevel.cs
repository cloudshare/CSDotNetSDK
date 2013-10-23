using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CSAPI
{
    /// <summary>
    /// Provides high API functionality using DTOs
    /// </summary>
    public class CSAPIHighLevel
    {
    	CSAPILowLevel _api;

        public CSAPIHighLevel(string apiKey, string apiId, string hostname = null)
        {
            _api = new CSAPILowLevel (apiKey, apiId, hostname);
        }

        #region EnvInfo

        public string GetEnvDetailsUrl(EnvsListElement env)
	    {
            var envList = ListEnvironments();
            return (from e in envList where e.envId == env.envId select e.view_url).FirstOrDefault();
	    }
	
	    public List<EnvStatus> GetEnvironmentStatusList(string filterSpecificUser="")
	    {
            var envList = ListEnvironments();
            return (from env in envList 
                    where filterSpecificUser == "" || filterSpecificUser.ToLower() == env.owner.ToLower() 
                    select GetEnvironmentState(env))
                        .ToList();
	    }
	
	    public EnvStatus GetEnvironmentState(EnvsListElement env)
	    {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId } };
			var json = _api.CallCSAPI("env","GetEnvironmentState", envStateParams);
		    return JsonConvert.DeserializeObject<EnvStatus>(json);  
	    }
     
        public List<SnapshotStatus> GetSnapshots(EnvsListElement env)
        {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId } };
            var json = _api.CallCSAPI("env", "GetSnapshots", envStateParams);
            return JsonConvert.DeserializeObject<List<SnapshotStatus>>(json);
        }

	    public List<EnvsListElement> ListEnvironments()
	    {
		    var json = _api.CallCSAPI("env","ListEnvironments", new Dictionary<string,string>());
            return JsonConvert.DeserializeObject<List<EnvsListElement>>(json);  
	    }

        public List<EnvStatus> ListEnvironmentsWithState()
        {
            var json = _api.CallCSAPI("env", "ListEnvironmentsWithState", new Dictionary<string, string>());
            return JsonConvert.DeserializeObject<List<EnvStatus>>(json);
        }

        public bool IsRevertable(EnvsListElement env)
        {
            return env.snapshot != "N/A" && env.snapshot != null;
        }

        public SnapshotDetails GetSnapshotDetails(string snapshotId)
        {
            var @params = new Dictionary<string, string>
                { 
                    { "snapshotId", snapshotId }
                };

            var json = _api.CallCSAPI("env", "GetSnapshotDetails", @params);
            return JsonConvert.DeserializeObject<SnapshotDetails>(json);
        }

        #endregion

        #region GeneralEnvActions

        public void ResumeEnvironment(EnvsListElement env)
	    {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "ResumeEnvironment", Params);
	    }

        public void RevertEnvironment(EnvsListElement env)
	    {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "RevertEnvironment", Params);
	    }

        public void DeleteEnvironment(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "DeleteEnvironment", Params);
        }

        public void ExtendEnvironment(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "ExtendEnvironment", Params);
        }

        public void SuspendEnvironment(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "SuspendEnvironment", Params);
        }

        public void RevertEnvironmentToSnapshot(EnvsListElement env, SnapshotStatus snapshot)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId }, { "SnapshotId", snapshot.SnapshotId } };
            _api.CallCSAPI("env", "RevertEnvironmentToSnapshot", Params);
        }

        public async Task ResumeEnvironmentAsync(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            await _api.CallCSAPIAsync("env", "ResumeEnvironment", Params);
        }

        public async Task RevertEnvironmentAsync(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            await _api.CallCSAPIAsync("env", "RevertEnvironment", Params);
        }

        public async Task DeleteEnvironmentAsync(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            await _api.CallCSAPIAsync("env", "DeleteEnvironment", Params);
        }

        public async Task ExtendEnvironmentAsync(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            await _api.CallCSAPIAsync("env", "ExtendEnvironment", Params);
        }

        public async Task SuspendEnvironmentAsync(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            await _api.CallCSAPIAsync("env", "SuspendEnvironment", Params);
        }

        public async Task RevertEnvironmentToSnapshotAsync(EnvsListElement env, SnapshotStatus snapshot)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId }, { "SnapshotId", snapshot.SnapshotId } };
            await _api.CallCSAPIAsync("env", "RevertEnvironmentToSnapshot", Params);
        }

        public PostponeInactivityActionResult PostponeInactivityAction(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            var json =_api.CallCSAPI("env", "PostponeInactivityAction", Params);
            return JsonConvert.DeserializeObject<PostponeInactivityActionResult>(json);
        }

        #endregion

        #region CreateEnvActions

        public List<TemplatesListElement> ListTemplates()
        {
            var json = _api.CallCSAPI("env", "ListTemplates", new Dictionary<string, string>());
            return JsonConvert.DeserializeObject<TemplatesList>(json).templatesList;
        }

        public bool AddVmFromTemplate(EnvsListElement env, TemplatesListElement template, string vmName, string vmDescription)
        {
            return InternalAddVMFromTemplate(env.envId, template.id, vmName, vmDescription);
        }

        private bool InternalAddVMFromTemplate (string envId, string templateId, string vmName, string vmDescription)
        {
            try
            {
                var Params = new Dictionary<string, string> { { "EnvId", envId }, 
                                                    { "TemplateVmId", templateId} ,
                                                    {"VmName",vmName},
                                                    {"VmDescription",vmDescription}
                };
                _api.CallCSAPI("env", "AddVmFromTemplate", Params);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception while adding a VM from template:\n" + e.Message);
                return false;
            }
        }

        public List<EnvPolicyListElement> CreateEntAppEnvOptions(string projectFilter = "", string blueprintFilter = "", string environmentPolicyDurationFilter = "")
        {
            var Params = new Dictionary<string, string> { { "ProjectFilter", projectFilter }, { "BlueprintFilter", blueprintFilter }, { "EnvironmentPolicyDurationFilter", environmentPolicyDurationFilter } };
            var json = _api.CallCSAPI("env", "CreateEntAppEnvOptions", Params);

            return JsonConvert.DeserializeObject<List<EnvPolicyListElement>>(json);
        }

        public void CreateEntAppEnv(EnvPolicyListElement environmentPolicy, SnapshotStatus snapshot, string environmentNewName = null, string projectFilter = "", string blueprintFilter = "", string environmentPolicyDurationFilter = "")
        {
            var Params = new Dictionary<string, string> { { "EnvironmentPolicyId", environmentPolicy.EnvironmentPolicyId }, { "SnapshotId", snapshot.SnapshotId }, { "ProjectFilter", projectFilter }, { "BlueprintFilter", blueprintFilter }, { "EnvironmentPolicyDurationFilter", environmentPolicyDurationFilter }, { "EnvironmentNewName", environmentNewName } };
            _api.CallCSAPI("env", "CreateEntAppEnv", Params);
        }
        
        public void CreateEmptyEntAppEnv(string envName, string projectName, string description = "none")
        {
            var Params = new Dictionary<string, string> { { "EnvName", envName }, { "ProjectName", projectName }, { "Description", description } };
            _api.CallCSAPI("env", "CreateEmptyEntAppEnv", Params);
        }

        #endregion

        #region Snapshots

        public List<BlueprintInfo> GetBlueprintsForPublish(EnvsListElement env)
        {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId } };
            var json = _api.CallCSAPI("env", "GetBlueprintsForPublish", envStateParams);
            return JsonConvert.DeserializeObject<List<BlueprintInfo>>(json);
        }

        public void MarkSnapshotDefault(EnvsListElement env, SnapshotStatus snapshot)
        {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId }, { "SnapshotId", snapshot.SnapshotId } };
            _api.CallCSAPI("env", "MarkSnapshotDefault", envStateParams);
        }

        public void EntAppTakeSnapshot(EnvsListElement env, string snapshotName, string description = "", bool setAsDefault = true)
        {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId }, { "SnapshotName", snapshotName }, { "Description", description }, { "SetAsDefault", setAsDefault ? "true" : "false" } };
            _api.CallCSAPI("env", "EntAppTakeSnapshot", envStateParams);
        }

        public void EntAppTakeSnapshotToNewBlueprint(EnvsListElement env, string snapshotName, string newBlueprintName, string description = "")
        {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId }, { "SnapshotName", snapshotName }, { "NewBlueprintName", newBlueprintName }, { "Description", description } };
            _api.CallCSAPI("env", "EntAppTakeSnapshotToNewBlueprint", envStateParams);
        }

        public void EntAppTakeSnapshotToExistingBlueprint(EnvsListElement env, string snapshotName, BlueprintInfo otherBlueprint, string description = "", bool setAsDefault = true)
        {
            var envStateParams = new Dictionary<string, string> { { "EnvId", env.envId }, { "SnapshotName", snapshotName }, { "OtherBlueprintId", otherBlueprint.ApiId }, { "Description", description }, { "SetAsDefault", setAsDefault ? "true" : "false" }, };
            _api.CallCSAPI("env", "EntAppTakeSnapshotToExistingBlueprint", envStateParams);
        }

        #endregion

        #region VmActions

        public bool DeleteVm(EnvsListElement env, VmStatus ms)
        {
            try
            {
                var Params = new Dictionary<string, string> { {"EnvId", env.envId}, {"VmId",ms.vmId}
                };
                _api.CallCSAPI("env", "DeleteVm", Params);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception while deleting a VM from environment:\n" + e.Message);
                return false;
            }
        }

        public bool EditMachineHardware(EnvsListElement env, VmStatus vm, int? numCpus, int? memorySizeMBs,
                                        int? diskSizeGBs)
        {
            var Params = new Dictionary<string, string> {{"EnvId", env.envId}, {"VmId", vm.vmId}};
            if (numCpus.HasValue)
                Params["NumCpus"] = numCpus.Value.ToString();
            if (memorySizeMBs.HasValue)
                Params["MemorySizeMBs"] = memorySizeMBs.Value.ToString();
            if (diskSizeGBs.HasValue)
                Params["DiskSizeGBs"] = diskSizeGBs.Value.ToString();
            var json = _api.CallCSAPI("Env", "EditMachineHardware", Params);
            var dto = JsonConvert.DeserializeObject<EditMachineHardwareDto>(json);
            return !dto.conflictsFound;
        }

        public bool RevertVm(EnvsListElement env, VmStatus ms)
        {
            try
            {
                var Params = new Dictionary<string, string> { {"EnvId", env.envId}, {"VmId",ms.vmId}
                };
                _api.CallCSAPI("env", "RevertVm", Params);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception while reverting a VM:\n" + e.Message);
                return false;
            }
        }

        public bool RebootVm(EnvsListElement env, VmStatus ms)
        {
            try
            {
                var Params = new Dictionary<string, string> { {"EnvId", env.envId}, {"VmId",ms.vmId}
                };
                _api.CallCSAPI("env", "RebootVm", Params);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception while rebooting a VM:\n" + e.Message);
                return false;
            }
        }

        public RemoteAccessFileResult GetRemoteAccessFile(EnvsListElement env, VmStatus ms, int desktopWidth, int desktopHeight, bool? isConsole)
        {
            var @params = new Dictionary<string, string>
                { 
                    { "EnvId", env.envId },
                    { "VmId", ms.vmId },
                    { "desktopWidth", desktopWidth.ToString() },
                    { "desktopHeight", desktopHeight.ToString() }
                };

            if (isConsole != null)
            {
                @params.Add("isConsole", isConsole.Value ? "true" : "false");
            }

            var json = _api.CallCSAPI("env", "GetRemoteAccessFile", @params);
            return JsonConvert.DeserializeObject<RemoteAccessFileResult>(json); 
        }

        public ExecutePathResult ExecutePath(EnvsListElement env, VmStatus ms, string path)
        {
            var @params = new Dictionary<string, string>
                { 
                    { "EnvId", env.envId },
                    { "VmId", ms.vmId },
                    { "Path", path }
                };

            var json = _api.CallCSAPI("env", "ExecutePath", @params);
            return JsonConvert.DeserializeObject<ExecutePathResult>(json);  
        }

        public DetailedCloudFoldersStatus MountAndFetchInfoExt(EnvsListElement env)
        {
            var @params = new Dictionary<string, string>
                { 
                    { "EnvId", env.envId },
                };

            var json = _api.CallCSAPI("env", "MountAndFetchInfoExt", @params);
            return JsonConvert.DeserializeObject<DetailedCloudFoldersStatus>(json);  
        }

        public ExecutePathExtResult ExecutePathExt(EnvsListElement env, VmStatus ms, string path)
        {
            var @params = new Dictionary<string, string>
                { 
                    { "EnvId", env.envId },
                    { "VmId", ms.vmId },
                    { "Path", path }
                };

            var json = _api.CallCSAPI("env", "ExecutePathExt", @params);
            return JsonConvert.DeserializeObject<ExecutePathExtResult>(json);
        }

        public CheckExecutionStatusResult CheckExecutionStatus(EnvsListElement env, VmStatus ms, string executionId)
        {
            var @params = new Dictionary<string, string>
                { 
                    { "EnvId", env.envId },
                    { "VmId", ms.vmId },
                    { "ExecutionId", executionId }
                };

            var json = _api.CallCSAPI("env", "CheckExecutionStatus", @params);
            return JsonConvert.DeserializeObject<CheckExecutionStatusResult>(json);
        }

        #endregion

        #region CloudFolders

        public CloudFoldersStatus GetCloudFoldersInfo()
        {
            var Params = new Dictionary<string, string> {};

            var json = _api.CallCSAPI("env", "GetCloudFoldersInfo", Params);
            return JsonConvert.DeserializeObject<CloudFoldersStatus>(json);  
        }

        public void Mount(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "Mount", Params);
        }

        public void Unmount(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            _api.CallCSAPI("env", "Unmount", Params);
        }

        public ExtendedCloudFoldersStatus MountAndFetchInfo(EnvsListElement env)
        {
            var Params = new Dictionary<string, string> { { "EnvId", env.envId } };
            var json = _api.CallCSAPI("env", "MountAndFetchInfo", Params);
            return JsonConvert.DeserializeObject<ExtendedCloudFoldersStatus>(json);
        }

        public RegeneratePasswordResult RegenerateCloudfoldersPassword()
        {
            var Params = new Dictionary<string, string> { };

            var json = _api.CallCSAPI("env", "RegenerateCloudfoldersPassword", Params);
            return JsonConvert.DeserializeObject<RegeneratePasswordResult>(json);
        }

        #endregion

        #region Login

        public string GetLoginUrl(string url)
        {
            var Params = new Dictionary<string, string> { { "Url", url } };

            var json = _api.CallCSAPI("env", "GetLoginUrl", Params);
            return JsonConvert.DeserializeObject<LoginElement>(json).login_url;  
        }

        public WhoAmIResult WhoAmI(string userId)
        {
            var Params = new Dictionary<string, string> { { "UserId", userId } };

            var json = _api.CallCSAPI("env", "WhoAmI", Params);
            return JsonConvert.DeserializeObject<WhoAmIResult>(json);
        }

        #endregion

        #region Admin

        public List<string> ListAllowedCommands()
        {
            var json = _api.CallCSAPI("Admin", "ListAllowedCommands");
            return JsonConvert.DeserializeObject<List<string>>(json);
        }

        #endregion

    }
}
