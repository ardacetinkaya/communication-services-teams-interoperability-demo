import React, { Component } from 'react';
import { CallClient } from "@azure/communication-calling";
import { LocalVideoStream, VideoStreamRenderer } from "@azure/communication-calling";
import { AzureCommunicationTokenCredential } from '@azure/communication-common';
import { ChatClient } from "@azure/communication-chat";

export class Home extends Component {
    static displayName = Home.name;

    call = null;
    callAgent = null;
    chatClient = null;
    chatThreadClient = null;

    placeCallOptions = null;
    deviceManager = null;
    localVideoStream = null;
    rendererLocal = null;
    rendererRemote = null;

    messages = '';

    constructor(props) {
        super(props);
        this.state = {
            initiated: false,
            btnInitText: "Initialize",
            userId: "",
            userName: "Unknown"
        };

        this.txtUserName = React.createRef();
        this.txtMeetingLink = React.createRef();
        this.txtMessageArea = React.createRef();
        this.lstMessages = React.createRef();
        this.lblCallStatus = React.createRef();

        this.btnMessageSend = React.createRef();
        this.btnStartVideo = React.createRef();
        this.btnStopVideo = React.createRef();
        this.btnJoinMeeting = React.createRef();
        this.btnHangUp = React.createRef();
        this.btnInit = React.createRef();

        this.mediaLocalVideo = React.createRef();
        this.mediaRemoteVideo = React.createRef();

    }

    init = async () => {
        try {
            this.setState({ btnInitText: "Initializing..." });
            const response = await fetch('token');
            const data = await response.json();

            this.setState({ userId: data.identity });

            const callClient = new CallClient();
            const tokenCredential = new AzureCommunicationTokenCredential({
                refreshProactivel: true,
                token: data.token,
                tokenRefresher: async (abortSignal) => {
                    const response = await fetch(`token/?id=${data.id}`, { method: "PUT" });
                    const data = response.json();
                    return data.token;
                }
            });

            this.chatClient = new ChatClient(
                data.endpoint,
                tokenCredential
            );

           
            if (this.txtUserName.current.value) this.setState({ userName: this.txtUserName.current.value })
            this.callAgent = await callClient.createCallAgent(tokenCredential, { displayName: this.state.userName });
            this.deviceManager = await callClient.getDeviceManager();
            this.setState({ initiated: true });

            this.btnJoinMeeting.current.disabled = !this.state.initiated;

            this.btnHangUp.current.disabled = this.state.initiated;
            this.btnStartVideo.current.disabled = this.state.initiated;
            this.btnStopVideo.current.disabled = this.state.initiated;
            this.setState({ btnInitText: "Initialized" });
            this.btnInit.current.disabled = this.state.initiated;

        } catch (e) {
            console.error(e);
        }

    }

    startMeeting = async () => {
        if (this.txtMeetingLink.current.value) {
            this.call = this.callAgent.join({ meetingLink: this.txtMeetingLink.current.value }, {});

            this.call.on('stateChanged', () => {
                this.lblCallStatus.current.innerText = this.call.state;
            })

            await this.chatClient.startRealtimeNotifications();

            //Subscribe the chat messages
            const userId = this.state.userId;
            this.chatClient.on("chatMessageReceived", async (e) => {

                if (e.sender.kind === "microsoftTeamsUser") {
                    await this.renderReceivedMessage(`${e.senderDisplayName}(w/MS Teams)`, e.message);
                } else if (e.sender.kind === "communicationUser" && e.sender.communicationUserId === userId) {
                    await this.renderSentMessage(e.senderDisplayName, e.message);
                }

            });

            //To get chat messages, need to get thread id.
            //Thread Id can be found in MeetingLink. Let's decode first...
            var decodedMeetingLink = decodeURIComponent(this.txtMeetingLink.current.value);
            var startIndex = decodedMeetingLink.indexOf("join/") + 5;
            var endIndex = decodedMeetingLink.lastIndexOf("/");
            var threadId = decodedMeetingLink.substring(startIndex, endIndex);

            this.chatThreadClient = await this.chatClient.getChatThreadClient(threadId);

            this.subscribeToRemoteParticipantInCall(this.call);

            this.btnHangUp.current.disabled = !this.state.initiated;
            this.btnJoinMeeting.current.disabled = this.state.initiated;
            this.btnStopVideo.current.disabled = this.state.initiated;
            this.btnStartVideo.current.disabled = !this.state.initiated;
        } else {
            console.warn("Meeting URL is required.");
        }
    }


    subscribeToRemoteParticipantInCall = async (callInstance) => {
        callInstance.on('remoteParticipantsUpdated', e => {
            e.added.forEach(p => {
                this.subscribeToParticipantVideoStreams(p);
            })
        });
        callInstance.remoteParticipants.forEach(p => {
            this.subscribeToParticipantVideoStreams(p);
        })
    }

    subscribeToParticipantVideoStreams = (participant) => {
        participant.on('videoStreamsUpdated', e => {
            e.added.forEach(v => {
                this.handleVideoStream(v);
            })
        });
        participant.videoStreams.forEach(v => {
            this.handleVideoStream(v);
        });
    }

    handleVideoStream = (remoteVideoStream) => {
        remoteVideoStream.on('isAvailableChanged', async () => {
            if (remoteVideoStream.isAvailable) {
                this.remoteVideoView(remoteVideoStream);
            } else {
                this.rendererRemote.dispose();
            }
        });
        if (remoteVideoStream.isAvailable) {
            this.remoteVideoView(remoteVideoStream);
        }
    }

    remoteVideoView = async (remoteVideoStream) => {
        this.rendererRemote = new VideoStreamRenderer(remoteVideoStream);
        const view = await this.rendererRemote.createView();
        this.mediaRemoteVideo.current.appendChild(view.target);
    }

    clear = async () => {
        if (this.initiated) {
            await this.leaveMeeting();
        }
        this.setState({ initiated: false });
        this.btnInit.current.disabled = this.state.initiated;
        this.setState({ btnInitText: "Initialize" });
    }

    leaveMeeting = async () => {
        if (this.rendererLocal) {
            this.rendererLocal.dispose();
        }

        if (this.rendererRemote) {
            this.rendererRemote.dispose();
        }


        await this.call.hangUp();

        this.btnHangUp.current.disabled = true;
        this.btnJoinMeeting.current.disabled = false;

        this.lblCallStatus.current.innerText = "-";

        this.btnStartVideo.current.disabled = true;
        this.btnStopVideo.current.disabled = true;


    }

    startVideo = async () => {
        const videoDevices = await this.deviceManager.getCameras();
        const videoDeviceInfo = videoDevices[0];
        this.localVideoStream = new LocalVideoStream(videoDeviceInfo);
        this.placeCallOptions = { videoOptions: { localVideoStreams: [this.localVideoStream] } };

        this.rendererLocal = new VideoStreamRenderer(this.localVideoStream);
        const view = await this.rendererLocal.createView();
        this.mediaLocalVideo.current.appendChild(view.target);

        await this.call.startVideo(this.localVideoStream);

        this.btnStartVideo.current.disabled = true;
        this.btnStopVideo.current.disabled = false;

    }


    stopVideo = async () => {
        await this.call.stopVideo(this.localVideoStream);
        this.rendererLocal.dispose();
        this.btnStartVideo.current.disabled = false;
        this.btnStopVideo.current.disabled = true;
    }

    renderReceivedMessage = async (from, message) => {
        this.messages += `<div class="d-flex text-left">
                            <div class="pr-2 pl-1"> <span class="small"><strong>${from}</strong></span>
                               <p class="small">${message}</p>
                            </div>
                          </div>`;

        this.lstMessages.current.innerHTML = this.messages;
    }

    renderSentMessage = async (from, message) => {
        this.messages += `<div class="d-flex text-right justify-content-end">
                            <div class="pr-2"> <span class="small"><strong>${from}</strong></span>
                               <p class="small">${message}</p>
                            </div>
                          </div>`;

        this.lstMessages.current.innerHTML = this.messages;
    }

    sendMessage = async () => {
        const sendMessageRequest = { content: this.txtMessageArea.current.value };
        const sendMessageOptions = { senderDisplayName: this.state.userName };
        await this.chatThreadClient.sendMessage(sendMessageRequest, sendMessageOptions);
    }

    renderMeeting(isInitianted) {
        if (isInitianted)
            return (
                <>
                    <div className="col-12 col-sm-4 col-md-4 col-lg-4 col-xl-4 text-center">
                        Hello <strong>{this.state.userName}</strong>
                        <p>Call state: <span ref={this.lblCallStatus} className="font-weight-bold"> - </span></p>
                        <div>
                            <button ref={this.btnJoinMeeting} type="button" className="btn btn-sm btn-primary mx-1" disabled={false} onClick={this.startMeeting}>Join Meeting</button>
                            <button ref={this.btnHangUp} type="button" className="btn btn-sm btn-primary mx-1" onClick={this.leaveMeeting}>Leave Meeting</button>
                        </div>
                        <div className="mb-3"></div>

                        <div style={{ border: "1px solid lightblue" }}>
                            <div style={{ height: "300px", width: "100%", overflowY: "scroll" }}>
                                <div ref={this.lstMessages} className="px-2">
                                    
                                </div>
                            </div>
                            <form className="form-container">
                                <div className="form-group">
                                    <textarea className="form-control" placeholder="Type message.." ref={this.txtMessageArea} required></textarea>
                                </div>
                                <div className="form-group float-right mt-2">
                                    <button type="button" className="btn btn-sm btn-primary mx-1" ref={this.btnMessageSend} onClick={this.sendMessage}>Send</button>
                                </div>
                            </form>

                        </div>
                    </div>
                    <div className="col-12 col-sm-4 col-md-4 col-lg-4 col-xl-4 text-center">
                        <strong>Local Video</strong>
                        <div style={{ border: "1px solid black", height: "200px", width: "100%" }}>
                            <div ref={this.mediaLocalVideo} style={{ backgroundColor: 'black', position: 'absolute', top: '50%', transform: 'translateY(-50%)' }}> </div>
                        </div>
                        <div className="mt-2">
                            <button ref={this.btnStartVideo} type="button" className="btn btn-sm btn-primary mx-1" onClick={this.startVideo}>Start Video</button>
                            <button ref={this.btnStopVideo} type="button" className="btn btn-sm btn-primary mx-1" onClick={this.stopVideo}>Stop Video</button>
                        </div>
                    </div>
                    <div className="col-12 col-sm-4 col-md-4 col-lg-4 col-xl-4 text-center">
                        <strong>Remote Video from MS Teams</strong>
                        <div style={{ border: "1px solid black", height: "200px", width: "100%" }}>
                            <div ref={this.mediaRemoteVideo} style={{ backgroundColor: 'black', position: 'absolute', top: '50%', transform: 'translateY(-50%)' }}></div>
                        </div>
                    </div>
                </>
            );
    }

    render() {
        const isInitiated = this.state.initiated;
        return (
            <div>
                <div className="row">

                    <div className="col-12 col-sm-6 col-md-6 col-lg-6 col-xl-6 text-center">
                        <input ref={this.txtUserName} type="text" placeholder="Your name" style={{ marginBottom: '1em', width: '100%' }} />
                    </div>
                    <div className="col-12 col-sm-6 col-md-6 col-lg-6 col-xl-6 text-center">
                        <input ref={this.txtMeetingLink} type="text" placeholder="Teams meeting link" style={{ marginBottom: '1em', width: '100%' }} />
                    </div>
                </div>
                <div className="text-center">
                    <button ref={this.btnInit} type="button" className="btn btn-sm btn-primary mx-2" onClick={this.init}>{this.state.btnInitText}</button>
                    <button type="button" className="btn btn-sm btn-primary mx-2" onClick={this.clear}>Clear</button>
                </div>
                <div className="row mt-4">
                    {this.renderMeeting(isInitiated)}
                </div>
            </div>

        );
    }
}