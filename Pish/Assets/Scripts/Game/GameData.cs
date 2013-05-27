using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//put in core
public class GameData : MonoBehaviour {
    public class Rank {
        public float criteria;
        public string spriteRef;
    }

    public class RankStatus {
        public float criteria;
        public string text;
    }

    public class Info {
        public string level;

        public bool ascending;

        public string rankNilRef;

        public string rankLabelFormat;
        public string rankBestLabelFormat;

        public Rank[] ranks;

        public bool scoreExists {
            get { return UserData.instance.HasKey(level + "score"); }
        }

        public float score {
            get { return UserData.instance.GetFloat(level + "score", ascending ? float.MaxValue : 0.0f); }
            set {
                SceneState.instance.SetValueFloat(level + "score", value, true);

                //save to best if it's the best
                float curBest = bestScore;

                if((ascending && value < curBest) || value > curBest) {
                    UserData.instance.SetFloat(level + "score_", value);
                }
            }
        }

        public float bestScore {
            get { return UserData.instance.GetFloat(level + "score_", ascending ? float.MaxValue : 0.0f); }
        }

        public string GetRankSpriteRef(float val) {
            string curRef = rankNilRef;

            foreach(Rank rank in ranks) {
                if(ascending) {
                    if(val <= rank.criteria)
                        curRef = rank.spriteRef;
                    else
                        break;
                }
                else {
                    if(val >= rank.criteria)
                        curRef = rank.spriteRef;
                    else
                        break;
                }
            }

            return curRef;
        }
    }

    public class FileData {
        public RankStatus[] fishnessRankings;
        public Info[] infos;
    }
    
    public TextAsset config;

    private static GameData mInstance = null;

    private RankStatus[] mFishnessRankings;
    private Dictionary<string, Info> mInfos;

    public static GameData instance {
        get { return mInstance; }
    }

    public string GetFishnessRanking(float val) {
        string ret = "";

        if(mFishnessRankings != null && mFishnessRankings.Length > 0) {
            ret = mFishnessRankings[0].text;
            for(int i = 1; i < mFishnessRankings.Length; i++) {
                if(val >= mFishnessRankings[i].criteria) {
                    ret = mFishnessRankings[i].text;
                }
                else {
                    break;
                }
            }
        }

        return ret;
    }

    public Info GetInfo(string level) {
        Info ret = null;
        if(!mInfos.TryGetValue(level, out ret)) {
            Debug.LogError("Unable to find info for: " + level);
        }

        return ret;
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            fastJSON.JSON.Instance.Parameters.UseExtensions = false;
            FileData fileData = fastJSON.JSON.Instance.ToObject<FileData>(config.text);

            mFishnessRankings = fileData.fishnessRankings;

            mInfos = new Dictionary<string, Info>(fileData.infos.Length);

            foreach(Info info in fileData.infos) {
                mInfos.Add(info.level, info);
            }
        }
    }
}
