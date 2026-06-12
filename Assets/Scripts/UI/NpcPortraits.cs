using UnityEngine;

namespace GaokaoSimulator.UI
{
    public enum NpcPortraitId
    {
        Guide,
        Mascot,
        Expert,
        GeneralSeniorMale,
        GeneralSeniorFemale,
        LawSeniorFemale,
        ComputerSeniorMale,
        MedicalSeniorFemale
    }

    public static class NpcPortraits
    {
        public static Sprite Load(NpcPortraitId id)
        {
            switch (id)
            {
                case NpcPortraitId.Guide:
                    return Resources.Load<Sprite>("UI/Guide/guide_character");
                case NpcPortraitId.Mascot:
                    return Resources.Load<Sprite>("UI/NPC/npc_mascot");
                case NpcPortraitId.Expert:
                    return Resources.Load<Sprite>("UI/NPC/npc_expert");
                case NpcPortraitId.GeneralSeniorMale:
                    return Resources.Load<Sprite>("UI/NPC/npc_senpai_general_m");
                case NpcPortraitId.GeneralSeniorFemale:
                    return Resources.Load<Sprite>("UI/NPC/npc_senpai_general_f");
                case NpcPortraitId.LawSeniorFemale:
                    return Resources.Load<Sprite>("UI/NPC/npc_senpai_law_f");
                case NpcPortraitId.ComputerSeniorMale:
                    return Resources.Load<Sprite>("UI/NPC/npc_senpai_cs_m");
                case NpcPortraitId.MedicalSeniorFemale:
                    return Resources.Load<Sprite>("UI/NPC/npc_senpai_med_f");
                default:
                    return null;
            }
        }
    }
}

