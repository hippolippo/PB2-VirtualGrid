using System;
using System.Collections;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using UnityEngine;
using System.Reflection;
using PolyTechFramework;
namespace VirtualGrid {
[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
[BepInDependency(PolyTechMain.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]

public class VirtualGrid : PolyTechMod {
public const string pluginGuid = "polytech.VirtualGrid";
public const string pluginName = "Virtual Grid";
public const string pluginVersion = "1.0.0";
public static ConfigEntry<bool> mEnabled;
public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> _keybindVirtualGrid;
public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> _keybindRotatedVirtualGrid;
public ConfigDefinition mEnabledDef = new ConfigDefinition(pluginVersion, "Enable/Disable Mod");
public ConfigDefinition _keybindVirtualGridDef = new ConfigDefinition(pluginVersion, "Virtual Grid Keybind");
public ConfigDefinition _keybindRotatedVirtualGridDef = new ConfigDefinition(pluginVersion, "Rotated Virtual Grid Keybind");

public static Vector3 normal = new Vector3(0f,0f,-1.9f);

public static bool keydown = false;
public static bool rkeydown = false;
public static VirtualGrid thing;


public override void enableMod(){
mEnabled.Value = true;
this.isEnabled = true;
}
public override void disableMod(){
mEnabled.Value = false;
this.isEnabled = false;
}
public override string getSettings(){
return "";
}
public override void setSettings(string settings){

}
public VirtualGrid(){
mEnabled = Config.Bind(mEnabledDef, true, new ConfigDescription("Controls if the mod should be enabled or disabled", null, new ConfigurationManagerAttributes {Order = 0}));
_keybindVirtualGrid = Config.Bind(_keybindVirtualGridDef, new BepInEx.Configuration.KeyboardShortcut(UnityEngine.KeyCode.Mouse3), new ConfigDescription("Keybind to use centered virtual grid", null, new ConfigurationManagerAttributes {Order = -1}));
_keybindRotatedVirtualGrid = Config.Bind(_keybindRotatedVirtualGridDef, new BepInEx.Configuration.KeyboardShortcut(UnityEngine.KeyCode.Mouse4), new ConfigDescription("Keybind to use 45-degree rotated centered virtual grid", null, new ConfigurationManagerAttributes {Order = -2}));
}
public void onEnableDisable(object sender, EventArgs e)
        {
            this.isEnabled = mEnabled.Value;
        }
void Awake(){
mEnabled = (ConfigEntry<bool>)Config[mEnabledDef];
mEnabled.SettingChanged += onEnableDisable;
this.isEnabled = mEnabled.Value;
thing = this;
Logger.LogInfo("Virtual Grid Methods Patched");
Harmony.CreateAndPatchAll(typeof(VirtualGrid));
}

void Update(){
    if (_keybindVirtualGrid.Value.IsDown()){
        normal = GameUI.m_Grid.transform.position;
        keydown = true;
    }else if(_keybindVirtualGrid.Value.IsUp()){
        keydown = false;
        GameUI.m_Grid.transform.position = normal;
    }
    if (_keybindRotatedVirtualGrid.Value.IsDown()){
        normal = GameUI.m_Grid.transform.position;
        rkeydown = true;
    }else if(_keybindRotatedVirtualGrid.Value.IsUp()){
        rkeydown = false;
        GameUI.m_Grid.transform.position = normal;
    }
}

[HarmonyPatch(typeof(GameUI), "SnapPosToGrid")]
[HarmonyPrefix]
private static bool GameUISnapPosToGridPrefixPatch(Vector3 worldPos, ref Vector3 __result){
    if(!rkeydown){
        float degrees = 0;
        Vector3 to = new Vector3(0, 0, degrees);
        GameUI.m_Grid.transform.eulerAngles = Vector3.Lerp(GameUI.m_Grid.transform.eulerAngles, to, 1f);
    }
    if(mEnabled.Value && rkeydown){
        Vector3 offset;
        offset = new Vector3(BridgeJointPlacement.m_SelectedJoint.transform.position.x, BridgeJointPlacement.m_SelectedJoint.transform.position.y, -1.9f);
        Vector3 difference = new Vector3(worldPos.x-offset.x, worldPos.y-offset.y, 0f);
        difference = Quaternion.Euler(0,0,45) * difference;
        float x = GameUI.RoundToNearestGridSquare(difference.x);
        float y = GameUI.RoundToNearestGridSquare(difference.y);
        Vector3 answer = new Vector3(x,y,0f);
        answer = Quaternion.Euler(0,0,-45) * answer;
        x = answer.x + offset.x;
        y = answer.y + offset.y;
        GameUI.m_Grid.transform.position = offset;
        float degrees = 45;
        Vector3 to = new Vector3(0, 0, degrees);
        GameUI.m_Grid.transform.eulerAngles = Vector3.Lerp(GameUI.m_Grid.transform.eulerAngles, to, 1f);
        __result.Set(x, y, worldPos.z);

    return false;
}
    if(mEnabled.Value && keydown){
        Vector3 offset;
        offset = new Vector3(BridgeJointPlacement.m_SelectedJoint.transform.position.x, BridgeJointPlacement.m_SelectedJoint.transform.position.y, -1.9f);
        float x = GameUI.RoundToNearestGridSquare(worldPos.x-offset.x) + offset.x;
        float y = GameUI.RoundToNearestGridSquare(worldPos.y-offset.y) + offset.y;
        GameUI.m_Grid.transform.position = offset;
        __result.Set(x, y, worldPos.z);

    return false;
}
return true;

}
}
}