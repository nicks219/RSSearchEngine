import * as React from 'react';
import {IMountedComponent, LoaderComponent} from "./loader.component.tsx";
import { menuHandler } from "../menu/menu.handler.tsx";
import {
    getStructuredTagsListResponse,
    getTagsCheckedUncheckedResponse,
    getTextResponse,
    getTitleResponse, setTextResponse
} from "../dto/dto.note.tsx";
import {ISimpleProps} from "../contracts/i.simple.props.tsx";
import {NoteResponseDto} from "../dto/note.response.dto.tsx";
import {ISubscribed} from "../contracts/i.subscribed.tsx";

interface IState {
    data: NoteResponseDto|null;
    time: number|null;
}

interface IProps extends ISimpleProps, ISubscribed<UpdateView> {
}

class UpdateView extends React.Component<ISimpleProps, IState> implements IMountedComponent {
    formId?: HTMLFormElement;
    mounted: boolean;

    public state: IState = {
        data: null,
        time: null
    }

    mainForm: React.RefObject<HTMLFormElement>;

    constructor(props: IProps) {
        super(props);
        this.formId = undefined;
        this.mounted = true;

        this.mainForm = React.createRef();
    }

    componentDidMount() {
        this.formId = this.mainForm.current == null ? undefined : this.mainForm.current;
        LoaderComponent.unusedPromise = LoaderComponent.getDataById(this, window.textId, LoaderComponent.updateUrl);
    }

    componentWillUnmount() {
        this.mounted = false;
    }

    render() {
        let checkboxes = [];
        if (this.state != null && this.state.data != null) {
            for (let i = 0; i < getStructuredTagsListResponse(this.state.data).length; i++) {
                checkboxes.push(<Checkbox key={"checkbox " + i + this.state.time} id={String(i)} jsonStorage={this.state.data} formId={undefined}/>);
            }
        }

        return (
            <div id="renderContainer">
                <form ref={this.mainForm} id="dizzy">
                    {checkboxes}
                    {this.state != null && this.state.data != null &&
                        <SubmitButton subscription={this} formId={this.formId} jsonStorage={this.state.data} id={undefined}/>
                    }
                </form>
                {this.state != null && this.state.data != null && getTextResponse(this.state.data) != null &&
                    <Message formId={this.formId} jsonStorage={this.state.data} id={undefined}/>
                }
            </div>
        );
    }
}

class Checkbox extends React.Component<ISimpleProps> {

    render() {
        let checked = getTagsCheckedUncheckedResponse(this.props) === "checked";
        let getGenreName = (i: number) => {
            return getStructuredTagsListResponse(this.props.jsonStorage)[i];
        };
        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox"
                    defaultChecked={checked} />
                <label htmlFor={this.props.id}>{getGenreName(Number(this.props.id))}</label>
            </div>
        );
    }
}

class Message extends React.Component<ISimpleProps> {
    constructor(props: ISimpleProps) {
        super(props);
        this.hideMenu = this.hideMenu.bind(this);
    }

    componentDidMount() {
        this.getCookie();
    }

    // именование кук ASP.NET: ".AspNetCore.Cookies"
    getCookie = () => {
        // куки выставляются в компоненте Login:
        const name = "rsse_auth";
        let matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/+^])/g, '\\$1') + "=([^;]*)"
        ));

        if (matches == null || decodeURIComponent(matches[1]) === 'false')
        {
            this.hideMenu();
        }
    }

    hideMenu() {
        if (this.props.formId) this.props.formId.style.display = menuHandler(this.props.formId.style.display);
        (document.getElementById("login")as HTMLElement).style.display = "block";
    }

    inputText = (e: string) => {
        setTextResponse(this.props.jsonStorage, e);
        this.forceUpdate();
    }

    render() {
        return (
            <div >
                <p />
                {this.props.jsonStorage != null ? ( getTextResponse(this.props.jsonStorage) != null ?
                    <div>
                        <h1 onClick={this.hideMenu}>
                            { getTitleResponse(this.props.jsonStorage) }
                        </h1>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                value={ getTextResponse(this.props.jsonStorage) } onChange={e => this.inputText(e.target.value)} />
                        </h5>
                    </div>
                    : "выберите заметку")
                    : "loading.."}
            </div>
        );
    }
}

class SubmitButton extends React.Component<IProps> {

    constructor(props: IProps) {
        super(props);
        this.submit = this.submit.bind(this);
    }

    submit(e: React.SyntheticEvent) {
        e.preventDefault();
        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        let formMessage = formData.get("msg");
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": getTitleResponse(this.props.jsonStorage),
            "commonNoteID": window.textId
        };
        let requestBody = JSON.stringify(item);
        LoaderComponent.unusedPromise = LoaderComponent.postData(this.props.subscription, requestBody, LoaderComponent.updateUrl);
    }

    componentWillUnmount() {
        // отменяй подписки и асинхронную загрузку
    }

    render() {
        return (
            <div id="submitStyle">
                <input type="checkbox" id="submitButton" className="regular-checkbox" onClick={this.submit} />
                <label htmlFor="submitButton">Сохранить</label>
            </div>
        );
    }
}

export default UpdateView;
