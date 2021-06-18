using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Steamworks;
using System.IO;
using System;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine.UI;
using TMPro;

public class SteamScript : MonoBehaviour
{
	

	
    public class modInfo
    {
        public ulong modId;
        public string title;
        public string modType;
    }
	modInfo modinfo;
	private CallResult<CreateItemResult_t> m_CreateItemResult;
	private CallResult<SubmitItemUpdateResult_t> m_itemUpdateResult;
	private CallResult<AddUGCDependencyResult_t> m_addDependecyResult;

	[SerializeField] TMP_InputField modPath;
	[SerializeField] TMP_InputField modTitle;
	[SerializeField] TMP_InputField modDescription;
	bool findModInfo;
	//string path = "";
	string modInfoPath = "";
	bool findModId = false;
	bool NeedUpLoad = false;
	//string modFilePath = path+"/modId1.txt";
	PublishedFileId_t PublishedFileId;
	private AppId_t appId;
	private void Start() {
		//modPath.text = path = EditorUtility.OpenFolderPanel("UploadMod","","");
		modinfo = new modInfo ();
		// if( modPath.text != "" && File.Exists(path))
		// {
		// 	fideModInfo();
		
		// }
		
	}
	private void OnEnable() {
		if (SteamManager.Initialized) {
			m_CreateItemResult = CallResult<CreateItemResult_t>.Create(CreateItemId);
			m_itemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(CheckItemUpdateResult);
			m_addDependecyResult = CallResult<AddUGCDependencyResult_t>.Create(CheckDependencyResult);
			appId.m_AppId = 1307550;
			
		}
	}

	private void fideModInfo()
	{
		modInfoPath = modPath.text+"/modInfo.txt";
		if(!File.Exists(modInfoPath))
		{
			FileStream file = File.Create(modInfoPath);
			file.Close();
		}

		if(File.Exists(modInfoPath))
		{
			string[] info = File.ReadAllText(modInfoPath).Split("\n".ToCharArray());
			string id = info[0];
			if(id != "")
			{
				PublishedFileId = new PublishedFileId_t(ulong.Parse(id));
				modinfo.modId = PublishedFileId.m_PublishedFileId;
				if(modinfo.modId != 0)
				{
					findModId = true;
				}
			}
			if(info.Length == 3)
			{
				modinfo.title = info[1];
				modinfo.modType = info[2];
			}

			findModInfo = true;
		}
		else
		{
			findModInfo = false;
		}
	}
	private void Update() {
		
		if(findModId && NeedUpLoad)
		{
			SteamUpLoadItem();
		}
		
	}

	public void SelectFolder()
	{
#if UNITY_EDITOR
		modPath.text = EditorUtility.OpenFolderPanel("UploadMod","","");
#else 
		OpenFileDialog FileDialog = new OpenFileDialog();
		FileDialog.FileName = modPath.text;
		//modPath.text = FileDialog.FileName;
		FileDialog.ShowDialog();
		modPath.text = FileDialog.FileName;
#endif
		if( modPath.text != "" )
		{
			fideModInfo();
		}
	}
	public void CreateAndUpload()
	{
		if( modPath.text != "" )
		{
			fideModInfo();
		}
		if(!findModInfo || !SteamManager.Initialized)
		{
			return;
		}
		NeedUpLoad = true;
		if(!findModId)
		{
			SteamCreateItem();
		}
		if(findModId)
		{
			SteamUpLoadItem();
		}
	}
	private void SteamCreateItem()
	{
		SteamAPICall_t handle = SteamUGC.CreateItem(appId,EWorkshopFileType.k_EWorkshopFileTypeCommunity);
		m_CreateItemResult.Set(handle);
	}
	private void SteamAddDependency()
	{
		SteamAPICall_t handle = SteamUGC.CreateItem(appId,EWorkshopFileType.k_EWorkshopFileTypeCommunity);
		m_CreateItemResult.Set(handle);
	}

	private void SteamUpLoadItem()
	{
		UGCUpdateHandle_t itemUpdateHandle = SteamUGC.StartItemUpdate(appId, PublishedFileId);
		string title = modTitle.text;
		//string modPath = "C:/test/New Unity Project/modId/slime_mod";
		modinfo.modId = PublishedFileId.m_PublishedFileId;
		if(SteamUGC.SetItemTitle(itemUpdateHandle, title))
		{
			modinfo.title = title;
			Debug.Log("SetItemTitle: " + "OK");
		}
		if(SteamUGC.SetItemDescription(itemUpdateHandle, modDescription.text))
		{
			Debug.Log("SetItemDescription: " + "OK");
		}
		// if(SteamUGC.SetItemMetadata(itemUpdateHandle,"C:/test/New Unity Project/modId/slime_mod/mono5.png"))
		// {
		// 	Debug.Log("SetItemMetadata: " + "OK");
		// }
		if(SteamUGC.SetItemContent(itemUpdateHandle,modPath.text))
		{
			Debug.Log("SetItemContent: " + "OK");
		}
		
		// if(SteamUGC.SetItemPreview(itemUpdateHandle,"C:/test/New Unity Project/modId/slime_mod/mono5.png"))
		// {
		// 	Debug.Log("SetItemPreview: " + "OK");
		// }
	
		CreateModInfo(modinfo);
		NeedUpLoad = false;
		SteamAPICall_t ItemUpdateResult = SteamUGC.SubmitItemUpdate(itemUpdateHandle, null);
		m_itemUpdateResult.Set(ItemUpdateResult);
		SteamAPICall_t addDependecyResult = SteamUGC.AddDependency(PublishedFileId,new PublishedFileId_t(2491629880));
		m_addDependecyResult.Set(addDependecyResult);
	}
	private void CreateItemId(CreateItemResult_t pCallback, bool bIOFailure) {
		
		Debug.Log("CreateItemResult: " + pCallback.m_eResult);
		if(!File.Exists(modInfoPath))
		{
			FileStream file = File.Create(modInfoPath);
			file.Close();
		}
		if(File.Exists(modInfoPath))
		{
			PublishedFileId = pCallback.m_nPublishedFileId;
			findModId = true;
			CreateModInfo(modinfo);
		}
		
	}
	private void CreateModInfo(modInfo info)
	{
		if(File.Exists(modPath.text+"/modInfo.txt"))
		{
			info.modType = "mod";
			string infoText = info.modId.ToString() + "\n" + info.title + "\n" + info.modType;
			File.WriteAllText(modPath.text+"/modInfo.txt", infoText);
		}
	}

	private void CheckItemUpdateResult(SubmitItemUpdateResult_t pCallback, bool bIODailure)
	{
		Debug.Log("UpdateItemResult: " + pCallback.m_eResult);
		
	}
	private void CheckDependencyResult(AddUGCDependencyResult_t pCallback, bool bIODailure)
	{
		Debug.Log("AddDependencyResult: " + pCallback.m_eResult);
		
	}
	public void QuitApp()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		UnityEngine.Application.Quit();
#endif
	}
}
