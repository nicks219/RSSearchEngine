import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {
    getStructuredTagsListResponse, getTagsCheckedUncheckedResponse, getTextResponse,
    getTitleResponse, setTextResponse
} from "../common/dto.handlers.tsx";
import {ISimpleProps, ISubscribed, IMountedComponent} from "../common/contracts.tsx";
import {NoteResponseDto} from "../dto/request.response.dto.tsx";
import {toggleMenuVisibility} from '../common/visibility.handlers.tsx';
import {useEffect, useReducer} from "react";

interface IState {
    data: NoteResponseDto|null;
    time: number|null;
}

interface ISubscribeProps extends ISimpleProps, ISubscribed<UpdateView> {
}

class UpdateView extends React.Component<ISimpleProps, IState> implements IMountedComponent {
    formId?: HTMLFormElement;
    mounted: boolean;

    public state: IState = {
        data: null,
        time: null
    }

    mainForm: React.RefObject<HTMLFormElement>;

    constructor(props: ISubscribeProps) {
        super(props);
        this.formId = undefined;
        this.mounted = true;

        this.mainForm = React.createRef();
    }

    componentDidMount() {
        this.formId = this.mainForm.current == null ? undefined : this.mainForm.current;
        Loader.unusedPromise = Loader.getDataById(this, window.textId, Loader.updateUrl);
    }

    componentWillUnmount() {
        this.mounted = false;
    }

    render() {
        let checkboxes = [];
        if (this.state != null && this.state.data != null) {
            for (let i = 0; i < getStructuredTagsListResponse(this.state.data).length; i++) {
                // без уникального ключа ${i}${this.state.time} при снятии последнего чекбокса он не перендерится после загрузки данных:
                checkboxes.push(<Checkbox key={`checkbox ${i}${this.state.time}`} id={String(i)} jsonStorage={this.state.data} formId={undefined}/>);
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

const Checkbox = (props: ISimpleProps) => {
    let checked = getTagsCheckedUncheckedResponse(props) === "checked";
    let getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.jsonStorage)[i];
    };
    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={checked}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

const Message = (props: ISimpleProps) => {
    //constructor(props: ISimpleProps) {
    //    super(props);
    //    this.hideMenu = this.hideMenu.bind(this);
    //}

    //componentDidMount() {this.getCookie();}
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    useEffect(() => {
        getCookie();
    }, []);

    // именование кук ASP.NET: ".AspNetCore.Cookies"
    const getCookie = () => {
        // куки выставляются в компоненте Login:
        const name = "rsse_auth";
        let matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/+^])/g, '\\$1') + "=([^;]*)"
        ));

        if (matches == null || decodeURIComponent(matches[1]) === 'false') {
            hideMenu();
        }
    }

    const hideMenu = () => {
        if (props.formId) props.formId.style.display = toggleMenuVisibility(props.formId.style.display);
        (document.getElementById("login") as HTMLElement).style.display = "block";
    }

    //inputText = (e: string) => {
    //    setTextResponse(this.props.jsonStorage, e);
    //    this.forceUpdate();
    //}

    const inputText = (e: string) => {
        setTextResponse(props.jsonStorage, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.jsonStorage != null ? (getTextResponse(props.jsonStorage) != null ?
                    <div>
                        <h1 onClick={hideMenu}>
                            {getTitleResponse(props.jsonStorage)}
                        </h1>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                      value={getTextResponse(props.jsonStorage)}
                                      onChange={e => inputText(e.target.value)}/>
                        </h5>
                    </div>
                    : "выберите заметку")
                : "loading.."}
        </div>
    );
}

const SubmitButton = (props: ISubscribeProps) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let formData = new FormData(props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        let formMessage = formData.get("msg");
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": getTitleResponse(props.jsonStorage),
            "commonNoteID": window.textId
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.subscription, requestBody, Loader.updateUrl);
    }

    return (
        <div id="submitStyle">
            <input type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Сохранить</label>
        </div>
    );
}

export default UpdateView;
