// Created by Domen Koneski
// https://github.com/domenkoneski/unity-jobs-callback

using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class JobHelper : MonoBehaviour {

    public static bool DebugLog = false;

    /// <summary>
    /// JobHelper instance. Get instance with GetInstance().
    /// </summary>
    private static JobHelper instance;
    
    /// <summary>
    /// Gets or creates JobHelper instance. Spawns GameObject with JobHelper component.
    /// </summary>
    /// <returns></returns>
    public static JobHelper GetInstance() {
        if (instance == null) {
            if (DebugLog) {
                Debug.Log("JobHelper instance is null. Creating instance.");
            }
            GameObject jobHelperGameObject = new GameObject("JobHelper_Internal");
            jobHelperGameObject.hideFlags |= HideFlags.HideInHierarchy;
            JobHelper.instance = jobHelperGameObject.AddComponent<JobHelper>();
        }

        return JobHelper.instance;
    }

    public class JobExecution {
        /// <summary>
        /// Job handle
        /// </summary>
        public JobHandle Handle;

        /// <summary>
        /// OnJobComplete delegate. Called after the job is completed.
        /// </summary>
        public OnJobComplete OnJobComplete;

        /// <summary>
        /// Frames taken for this job to complete.
        /// </summary>
        public int FramesTaken = -1;

        /// <summary>
        /// Job duration time from schedule to completion.
        /// </summary>
        public float Duration;

        /// <summary>
        /// Is job working?
        /// </summary>
        public bool JobWorking;

        /// <summary>
        /// Complete this job immediatelly in LateUpdate()?
        /// </summary>
        public bool CompleteInLateUpdate;

        /// <summary>
        /// Was IJobDisposable.OnDispose() called?
        /// </summary>
        public bool Disposed;

        /// <summary>
        /// Job tag.
        /// </summary>
        public string JobTag;
        
        private IJobDisposable Job;

        public JobExecution(IJobDisposable job, JobHandle handle, OnJobComplete onJobComplete, bool completeInLateUpdate, string jobTag)
        {
            this.Job = job;
            this.Handle = handle;
            this.OnJobComplete = onJobComplete;
            this.CompleteInLateUpdate = completeInLateUpdate;
            this.JobTag = jobTag != null ? jobTag : GetHashCode().ToString();
            this.JobWorking = true;
        }

        public bool Complete() {
            if (this.JobWorking) {
                this.Handle.Complete();
                this.JobWorking = false;
                this.OnJobComplete(this);
                return true;
            }
            return false;
        }

        public void Dispose() {
            if (this.Disposed) {
                return;
            }

            this.Job.OnDispose();
            this.Disposed = true;
        }
    }

    public interface IJobDisposable {
        void OnDispose();
    }

    public delegate void OnJobComplete(JobExecution execution);

    List<JobExecution> _scheduledJobs = new List<JobExecution>();
    List<JobExecution> _completedJobs = new List<JobExecution>();

    void Update() {
        for (int i = 0; i < this._scheduledJobs.Count; i++) {
            JobExecution execution = this._scheduledJobs[i];
            execution.FramesTaken ++;
            execution.Duration += Time.unscaledDeltaTime;
        }

        for (int i = this._completedJobs.Count; i >= 0; i--) {
            JobExecution execution = this._completedJobs[i];
            if (execution.Disposed) {
                this._completedJobs.RemoveAt(i);
            }
        }
    }

    void LateUpdate() {
        for (int i = this._scheduledJobs.Count - 1; i >= 0; i--) {
            JobExecution execution = this._scheduledJobs[i];
            if (execution.Handle.IsCompleted && execution.JobWorking || execution.CompleteInLateUpdate) {
                execution.Complete();
                
                if (DebugLog) {
                    Debug.LogFormat("Job {0} has been completed. Removing it from scheduled jobs.", execution.JobTag);
                }
                
                this._completedJobs.Add(execution);
                this._scheduledJobs.RemoveAt(i);
            }
        }
    }

    void OnDestroy() {
        for (int i = 0; i < this._scheduledJobs.Count; i++) {
            this._scheduledJobs[i].Handle.Complete();
            this._scheduledJobs[i].Dispose();
        }
        for (int i = 0; i < this._completedJobs.Count; i++) {
            this._completedJobs[i].Dispose();
        }
    }

    JobExecution AddJob(IJobDisposable job, JobHandle handle, OnJobComplete onJobComplete, bool completeImmediatelly = false, string tag) {
        JobExecution execution = new JobExecution(job, handle, onJobComplete, completeImmediatelly, tag);
        this._scheduledJobs.Add(execution);
        if (DebugLog) {
            Debug.LogFormat("Job {0} has been scheduled and waiting for completion.", execution.JobTag);
        }
        return execution;
    }

    /// <summary>
    /// Adds scheduled job to the system.
    /// </summary>
    /// <param name="job">IJobDisposable</param>
    /// <param name="handle">Job Handle with scheduled job</param>
    /// <param name="onJobComplete">Delegate which is invoked after job is completed</param>
    /// <param name="completeImmediatelly">Complete this job immediately in the next LateUpdate() call?</param>
    public static JobExecution AddScheduledJob(IJobDisposable job, JobHandle handle, OnJobComplete onJobComplete, bool completeImmediatelly = false, string tag = null) {
        return JobHelper.GetInstance().AddJob(job, handle, onJobComplete, completeImmediatelly, tag);
    }

    /// <summary>
    /// Clears and disposes all completed jobs.
    /// </summary>
    public static void ClearAndDisposeCompletedJobs() {
        for (int i = 0; i < JobHelper.GetInstance()._completedJobs.Count; i++) {
            JobHelper.GetInstance()._completedJobs[i].Dispose();
        }
        JobHelper.GetInstance()._completedJobs.Clear();
    }
}
