using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploreGameData : GameData.TypeData {
    public override int GetScore(float val) {
        return 0;
    }

    public override string GetScoreString(bool best) {
        return "";
    }
}

public class FishingGameData : GameData.TypeData {
    public string scoreFormat;
    public string bestScoreFormat;

    public override int GetScore(float val) {
        return Mathf.RoundToInt(val);
    }

    public override string GetScoreString(bool best) {
        return string.Format(best ? bestScoreFormat : scoreFormat, GetScore(GetValue(best)));
    }
}

public class FishingCollectData : GameData.TypeData {
    public string format;
    public string bestFormat;

    public float criteriaScore = 1000.0f;
    public float criteriaBonusMod = 2.0f;

    public override int GetScore(float val) {
        float score = 0.0f;
        float criteriaMax = 0.0f;
        foreach(float criteria in criterias) {
            if(val >= criteria) {
                score += criteriaScore;
                criteriaMax += criteriaMax;
            }
        }

        score += (val - criteriaMax) * criteriaBonusMod;

        return Mathf.RoundToInt(score);
    }

    public override string GetScoreString(bool best) {
        float val = GetValue(best);
        int centi = Mathf.RoundToInt(val * 100.0f);
        int seconds = Mathf.RoundToInt(val);
        int minutes = seconds / 60;
        return string.Format(best ? bestFormat : format, minutes % 60, seconds % 60, centi % 100);
    }
}

//put in core
public class GameData : MonoBehaviour {
    public struct HIScore {
        public int score;
        public string rank;
    }

    public struct LevelScore {
        public string medalSpriteRef;
        public string text;
    }

    public abstract class TypeData {
        public string level = "";
        public float[] criterias = {}; //0 = bronze, 1 = silver, 2 = gold

        //for time related types, this should return true
        public virtual bool ascending { get { return false; } }

        public abstract int GetScore(float val);

        public abstract string GetScoreString(bool best);

        //0 = nil, 1 = bronze, 2 = silver, 3 = gold
        public int GetMedalIndex(bool best) {
            int ret = 0;
            float val = GetValue(best);

            for(int i = 0; i < criterias.Length; i++) {
                if((ascending && val <= criterias[i]) || val >= criterias[i]) {
                    ret = i + 1;
                }
                else {
                    break;
                }
            }

            return ret;
        }

        //make sure to save on user's 'last' and 'best' data
        public void SaveValue(float val) {
            UserData.instance.SetFloat(level + "_v", val);

            //save to best if it's the best
            float curBestVal = GetValue(true);

            if((ascending && val < curBestVal) || val > curBestVal) {
                UserData.instance.SetFloat(level + "_bv", val);
            }
        }

        //best=false: last user's value data
        public float GetValue(bool best) {
            return UserData.instance.GetFloat(level + (best ? "_bv" : "_v"), ascending ? float.MaxValue : 0.0f);
        }
    }

    public class Rank {
        public int criteria;
        public string text;
    }

    public class FileData {
        public string scoreFormat;
        public string rankFormat;

        public Rank[] ranks;

        public string[] medalSpriteRefs; // {nil, bronze, silver, gold}

        public TypeData[] levels;
    }

    public TextAsset config;

    private static GameData mInstance = null;

    private string mScoreFormat;
    private string mRankFormat;

    private Rank[] mRankings;
    private string[] mMedalSpriteRefs;
    private TypeData[] mLevels;
    private Dictionary<string, TypeData> mLevelNameRefs;

    public static GameData instance {
        get { return mInstance; }
    }

    public string GetLevelName(int level) {
        if(level < mLevels.Length) {
            return mLevels[level].level;
        }

        Debug.LogError("Invalid level index: " + level);
        return "";
    }

    public HIScore GetHIScore() {
        HIScore total = new HIScore() { score = 0, rank = "" };

        foreach(TypeData dat in mLevels) {
            total.score += dat.GetScore(dat.GetValue(true));
        }

        foreach(Rank rank in mRankings) {
            if(total.score >= rank.criteria)
                total.rank = rank.text;
            else
                break;
        }

        return total;
    }

    public void GetHIScoreString(out string score, out string rank) {
        HIScore hiScore = GetHIScore();
        score = string.Format(mScoreFormat, hiScore.score);
        rank = string.Format(mRankFormat, hiScore.rank);
    }

    LevelScore _GetLevelScore(TypeData dat, bool best) {
        LevelScore ret = new LevelScore() { medalSpriteRef = "", text = "" };
        
        int medalInd = dat.GetMedalIndex(best);
        ret.medalSpriteRef = mMedalSpriteRefs[medalInd];
        ret.text = dat.GetScoreString(best);

        return ret;
    }

    public LevelScore GetLevelScore(string levelName, bool best) {
        TypeData dat = null;
        if(!mLevelNameRefs.TryGetValue(levelName, out dat)) {
            Debug.LogError("Level score data not found: " + levelName);
            return new LevelScore() { medalSpriteRef = "", text = "" };
        }

        return _GetLevelScore(dat, best);
    }

    public LevelScore GetLevelScore(int level, bool best) {
        if(level < mLevels.Length) {
            return _GetLevelScore(mLevels[level], best);
        }

        Debug.LogError("Invalid level index: " + level);
        return new LevelScore() { medalSpriteRef = "", text = "" };
    }

    public void SaveLevelScore(string levelName, float val) {
        TypeData dat = null;
        if(!mLevelNameRefs.TryGetValue(levelName, out dat)) {
            return;
        }

        dat.SaveValue(val);
    }

    public void SaveLevelScore(int level, float val) {
        if(level < mLevels.Length) {
            TypeData dat = mLevels[level];
            dat.SaveValue(val);
        }
        else {
            Debug.LogError("Invalid level index: " + level);
        }
    }

    void OnDestroy() {
        if(mInstance == this)
            mInstance = null;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            fastJSON.JSON.Instance.Parameters.UseExtensions = true;
            fastJSON.JSON.Instance.Parameters.UsingGlobalTypes = false;
            FileData fileData = fastJSON.JSON.Instance.ToObject<FileData>(config.text);

            mScoreFormat = fileData.scoreFormat;
            mRankFormat = fileData.rankFormat;

            mRankings = fileData.ranks;
            mMedalSpriteRefs = fileData.medalSpriteRefs;
            mLevels = fileData.levels;

            mLevelNameRefs = new Dictionary<string, TypeData>(mLevels.Length);
            foreach(TypeData dat in mLevels) {
                mLevelNameRefs[dat.level] = dat;
            }
        }
    }
}
