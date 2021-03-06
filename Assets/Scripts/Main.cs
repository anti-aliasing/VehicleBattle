﻿using GameClient.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient
{
    public class Main : MonoBehaviour
    {
        private void Awake()
        {
            //启动整个PureMVC框架
            AppFacade.Instance.StartUp();
            //开启网络服务
            StartCoroutine(StartNetworkService());
        }

        private IEnumerator StartNetworkService()
        {
            yield return null;  //这里要等所有MonoBehaviour的Start执行完
            var context = SynchronizationContext.Current ?? throw new ArgumentNullException(nameof(SynchronizationContext));
            //启动网络服务
            NetworkService.Instance.OnConnectStateChange += (connectState) => context.Post((state) => AppFacade.Instance.SendNotification(NotifyConsts.CommonNotification.UpdateConnState, connectState, nameof(ConnectState)), null);     //确保SendNotification的调用在主线程
            NetworkService.Instance.Start();
            yield return null;
            //单独开一个线程分发命令
            Task.Factory.StartNew(new CommandDispatcher().StartDispatch, context, TaskCreationOptions.LongRunning);   //LongRunning会创建线程池外的一个独立线程
        }

        private void OnDestroy()
        {
            //关闭网络服务
            NetworkService.Instance.Close();
        }
    }

}
