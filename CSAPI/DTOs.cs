using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// used http://json2csharp.com/ in order to generate the below

namespace CSAPI
{
    public class ApiResponse
    {
        public object data { get; set; }
        public int remaining_api_calls { get; set; }

        public string status_code { get; set; }
        public string status_text { get; set; }
        public string status_additional_data { get; set; }
    }

    public class EnvsListElement
    {
        public string envId { get; set; }
        public string envToken { get; set; }

        public string name { get; set; }
        public string description { get; set; }

        public int status_code { get; set; }
        public string status_text { get; set; }
        
        public string organization { get; set; }
        public string owner { get; set; }
        public string licenseValid { get; set; }
        public bool invitationAllowed { get; set; }
        public string expirationTime { get; set; }
        public string view_url { get; set; }
        
        public string snapshot { get; set; }
        public string blueprint { get; set; }
        public string project { get; set; }
        public string environmentPolicy { get; set; }
    }

    public class DetailedEnvsListElement : EnvsListElement
    {
        public List<VmStatus> vms { get; set; }
    }

    public class EnvStatus : DetailedEnvsListElement
    {
        public AvailableActions available_actions { get; set; }
        public EnvResources resources { get; set; }
    }

    public class EnvsList
    {
        public List<EnvsListElement> envsList { get; set; }
    }

    public class VmStatus
    {
        public string vmId { get; set; }
        public string vmToken { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string os { get; set; }
        public string IP { get; set; }
        public string FQDN { get; set; }
        public int status_code { get; set; }
        public string status_text { get; set; }
        public int progress { get; set; }
        public string webAccessUrl { get; set; }
        public string url { get; set; }
        public string image_url { get; set; }
    }

    public class AvailableActions
    {
        public bool add_vms { get; set; }
        public bool delete_vm { get; set; }
        public bool reboot_vm { get; set; }
        public bool revert_vm { get; set; }
        public bool resume_environment { get; set; }
        public bool revert_environment { get; set; }
        public bool take_snapshot { get; set; }
    }

    public class EnvResources
    {
        public int cpu_in_use { get; set; }
        public int cpu_qouta { get; set; }
        public int disk_size_in_use_mb { get; set; }
        public int disk_size_qouta_mb { get; set; }
        public int total_memory_in_use_mb { get; set; }
        public int total_memory_qouta_mb { get; set; }
    }

    public class TemplatesListElement
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int num_cpus { get; set; }
        public int disk_size_gb { get; set; }
        public int memory_size_mb { get; set; }
        public string image_url { get; set; }
        public bool is_singleton { get; set; }
        public int os_type { get; set; }
        public string os_type_string { get; set; }
        public string tags { get; set; }
        public List<object> categories { get; set; }
    }

    public class TemplatesList
    {
        public List<TemplatesListElement> templatesList { get; set; }
    }

    public class Credentials
    {
        public String ApiId { get; set; }
        public String ApiKey { get; set; }
    }

    public class SnapshotStatus
    {
        public string Author { get; set; }
        public string Comment { get; set; }
        public string CreationTime { get; set; }
        public bool IsDefault { get; set; }
        public bool IsLatest { get; set; }
        public string Name { get; set; }
        public string SnapshotId { get; set; }
    }

    public class BlueprintStatus
    {
        public string Name { get; set; }
        public List<SnapshotStatus> Snapshots { get; set; }
    }

    public class EnvPolicyListElement
    {
        public List<BlueprintStatus> Blueprints { get; set; }
        public string EnvironmentPolicyDuration { get; set; }
        public string EnvironmentPolicyId { get; set; }
        public List<string> Organizations { get; set; }
        public string Project { get; set; }
    }

    public class CloudFoldersStatus
    {
        public string host { get; set; }
        public string password { get; set; }
        public string quota_in_use_gb { get; set; }
        public string total_quota_gb { get; set; }
        public string uri { get; set; }
        public string user { get; set; }
        public string private_folder_name { get; set; }
    }

    public class ExtendedCloudFoldersStatus : CloudFoldersStatus
    {
        public string linuxFolder { get; set; }
        public string mounted_folder_token { get; set; }
        public string windowsFolder { get; set; }
    }

    public class DetailedCloudFoldersStatus : ExtendedCloudFoldersStatus
    {
        public bool? isActionComplete { get; set; }
    }

    public class RegeneratePasswordResult
    {
        public string new_password { get; set; }
        public string new_ftp_uri { get; set; }
    }

    public class BlueprintInfo
    {
        public string ApiId { get; set; }
        public string Name { get; set; }
    }

    public class LoginElement
    {
        public string login_url { get; set; }
    }

    public class WhoAmIResult
    {
        public string first_name { get; private set; }
        public string last_name { get; private set; }
        public string email { get; private set; }
        public string company { get; private set; }
        public string phone { get; private set; }
        public string job_title { get; private set; }
    }

    public class ExecutePathResult
    {
        public string executed_path { get; set; }
    }

    public class PostponeInactivityActionResult
    {
        public bool is_success { get; private set; }
        public string message { get; private set; }
    }

    public class RemoteAccessFileResult
    {
        public string rdp { get; set; }
        public string clearTextPassword { get; set; }
    }

    public class SnapshotDetails
    {
        public string snapshotId { get; set; }
        public string name { get; set; }
        public string creationTime { get; set; }
        public string author { get; set; }
        public string comment { get; set; }
        public bool isDefault { get; set; }
        public bool isLatest { get; set; }
        public IList<MachineDetails> machineList { get; set; }
        public string url { get; set; }

        public class MachineDetails
        {
            public string name { get; set; }
            public string os { get; set; }
            public string internalAdresses { get; set; }
            public long? memory_mb { get; set; }
            public long? diskSize_mb { get; set; }
            public int? cpu_count { get; set; }
            public string description { get; set; }
            public string user { get; set; }
            public string password { get; set; }
        }
    }

    public class ExecutePathExtResult
    {
        public string executionId { get; set; }
    }

    public class CheckExecutionStatusResult
    {
        public int? error_code { get; set; }
        public bool? success { get; set; }
        public string standard_output { get; set; }
        public string standard_error { get; set; }
        public string executed_path { get; set; }
    }
	
	public class EditMachineHardwareDto
    {
        public bool conflictsFound { get; set; }
        public List<string> conflicts { get; set; }
    }
}
