﻿using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RalseiMod.Modules
{
    public abstract class SharedBase
    {
        public abstract string ConfigName { get; }
        public virtual bool isEnabled { get; } = true;
        public static ManualLogSource Logger => Log._logSource;
        public abstract AssetBundle assetBundle { get; }

        public abstract void Hooks();
        public abstract void Lang();

        public virtual void Init()
        {
            ConfigManager.HandleConfigAttributes(GetType(), ConfigName, Config.MyConfig);
            Hooks();
            Lang();
        }

        public string d(float f) => (f * 100f).ToString() + "%";

        public string m(float f) => f + "m";

        public string s(float f, string suffix) => f + (suffix.StartsWith("{Stack}") ? "" : " ") + suffix + (Mathf.Abs(f) > 1 ? "s" : string.Empty);

        public static string StackDesc(float init, float stack, Func<float, string> initFn)
        {
            return StackDesc(init, stack, initFn, f => f.ToString());
        }

        public static string StackDesc(float init, float stack, Func<float, string> initFn, Func<float, string> stackFn)
        {
            if (init == 0 && stack == 0) return string.Empty;
            string ret = initFn(init);
            if (stack != 0) ret = ret.Replace("{Stack}", " <style=cStack>(" + (stack > 0 ? "+" : string.Empty) + stackFn(stack) + " per stack)</style>");
            else ret = ret.Replace("{Stack}", "");
            return ret;
        }

        public static float StackAmount(float init, float stack, float count, float isHyperbolic = 0f)
        {
            if (count <= 0) return 0;
            float ret = init + (stack * (count - 1));
            if (isHyperbolic != 0) ret = GetHyperbolic(init, isHyperbolic, ret);
            return ret;
        }

        public static float GetHyperbolic(float firstStack, float cap, float chance) // Util.ConvertAmplificationPercentageIntoReductionPercentage but Better :zanysoup:
        {
            if (firstStack >= cap) return cap * (chance / firstStack); // should not happen, but failsafe
            float count = chance / firstStack;
            float coeff = 100 * firstStack / (cap - firstStack); // should be good
            return cap * (1 - (100 / ((count * coeff) + 100)));
        }
    }
}
