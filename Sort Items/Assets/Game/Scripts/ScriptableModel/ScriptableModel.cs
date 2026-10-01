using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace SortItems
{
    [System.Serializable]
    public class ScriptableModel<TModel> : ScriptableObject, IStorable where TModel:Model, new()
    {
        [SerializeField] protected TModel _model;
        private static string GetKey(string name) => "save_" + name;

        public UnityEvent OnLoad;
        public UnityEvent OnSave;

        public TModel Model
        {
            get => _model;
            set => _model = value;
        }

        public bool Load()
        {
            string text; 
            #if UNITY_WEBGL && !UNITY_EDITOR 
                if (!PlayerPrefs.HasKey(GetKey(name)))
                {
                    Debug.Log("Save " + GetKey(name) + " not exist");
                    return false;
                }
                text = PlayerPrefs.GetString(GetKey(name));
            #else                                              
                if (File.Exists(GetStoragePath(name)) == false)
                {
                    Debug.Log("File " + GetStoragePath(name) + " not exist");
                    return false;
                }
                text = File.ReadAllText(GetStoragePath(name)); 
            #endif                                          

            TModel model = new TModel();             
            JsonUtility.FromJsonOverwrite(text,model);

            Model.OnChange.RemoveAllListeners();
            Model = model;

            OnLoad.Invoke();
            return true;
        }

        public bool Save()
        {
            try
            {
                var text = JsonUtility.ToJson(Model);
                #if UNITY_WEBGL && !UNITY_EDITOR                    
                    PlayerPrefs.SetString(GetKey(name), text);           
                    PlayerPrefs.Save();                                  
                #else                                                
                File.WriteAllText(GetStoragePath(name), text);     
                #endif     
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return false;
            }
            OnSave.Invoke();
            return true;
        }

        protected static string GetStoragePath(string name)
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar + name + ".json";
        }

    }
}
