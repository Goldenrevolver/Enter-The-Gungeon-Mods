using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SkipToFoyer
{
    internal class Updater : MonoBehaviour
    {
        private bool startedQuickStart = false;

        protected void Update()
        {
            //TitleDioramaController z = GameObject.FindObjectOfType<TitleDioramaController>();
            //if (z != null)
            //{
            //    typeof(TitleDioramaController).GetField("m_rushed", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(z, true);
            //}

            if (!startedQuickStart)
            {
                FinalIntroSequenceManager x = GameObject.FindObjectOfType<FinalIntroSequenceManager>();
                if (x != null)
                {
                    startedQuickStart = true;
                    x.StartCoroutine(WaitAndSkip(x));
                }
            }
        }

        private IEnumerator WaitAndSkip(FinalIntroSequenceManager x)
        {
            yield return null;
            //typeof(FinalIntroSequenceManager).GetField("m_skipLegend", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(x, true);
            typeof(FinalIntroSequenceManager).GetField("m_isDoingQuickStart", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(x, true);
            typeof(FinalIntroSequenceManager).GetField("m_skipCycle", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(x, true);
            var y = typeof(FinalIntroSequenceManager).GetMethod("DoQuickStart", BindingFlags.NonPublic | BindingFlags.Instance);
            x.StartCoroutine(y.Name, 0);
        }
    }
}