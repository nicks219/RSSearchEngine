import * as React from 'react';
import { Loader } from "../common/loader.tsx";
import { useParams } from "react-router-dom";
import {
    getCommonNoteId, getStructuredTagsListResponse,
    getTextResponse, getTitleResponse
} from "../common/dto.handlers.tsx";
import { createRoot, Root } from "react-dom/client";
import { NoteResponseDto } from "../dto/request.response.dto.tsx";
import {IMountedComponent, ISimpleProps, ISubscribed} from "../common/contracts.tsx";
import {toggleMenuVisibility} from "../common/visibility.handlers.tsx";

interface IState {
    data: NoteResponseDto|null;
}

interface IProps extends ISimpleProps, ISubscribed<ReadViewParametrized> {
}

export const ReadView = () => {
    const params = useParams();
    return <ReadViewParametrized textId={params.textId} />;
}

class ReadViewParametrized extends React.Component<{textId: string|undefined}, IState> implements IMountedComponent {
    formId?: HTMLFormElement;
    mounted: boolean;
    displayed: boolean;

    static searchButtonOneElement = document.querySelector("#searchButton1") ?? document.createElement('searchButton1');
    static searchButtonRoot: Root = createRoot(this.searchButtonOneElement);

    readFromCatalog: boolean;

    public state: IState = {
        data: null
    }

    mainForm: React.RefObject<HTMLFormElement>;

    constructor(props: {textId: string|undefined}) {
        // из "каталога" я попаду в конструктор, далее в DidMount, id песни { data: 1 } будет в props:
        super(props);

        this.formId = undefined;
        this.mounted = true;
        this.displayed = false;

        this.readFromCatalog = false;
        this.mainForm = React.createRef();
    }

    componentDidMount() {
        this.formId = this.mainForm.current == null ? undefined : this.mainForm.current;

        if (!this.readFromCatalog) {
            Loader.unusedPromise = Loader.getData(this, Loader.readUrl);
        }
        else{
            // если заметка вызвана из каталога, то не обновляем содержимое компонента, а также убираем чекбоксы и логин:
            if (this.formId) this.formId.style.display = "none";
            (document.getElementById("login")as HTMLElement).style.display = "none";
        }

        this.readFromCatalog = false;
    }

    componentDidUpdate() {
        ReadViewParametrized.searchButtonRoot.render(
            <div>
                <SubmitButton subscription={this} formId={this.formId} jsonStorage={undefined} id={undefined}/>
            </div>
        );
        (document.getElementById("header")as HTMLElement).style.backgroundColor = "#e9ecee";//???
    }

    componentWillUnmount() {
        this.mounted = false;
        // убираем отображение кнопки "Поиск":
        ReadViewParametrized.searchButtonRoot.render(
            <div>
            </div>
        );
    }

    render() {
        // читаем заметку из каталога:
        if (this.props./*match.params.*/textId && !this.displayed) {
            console.log("Get text id from path params: " + this.props./*match.params.*/textId);

            const item = {
                "tagsCheckedRequest": []
            };
            let requestBody = JSON.stringify(item);
            let id = this.props./*match.params.*/textId;
            this.readFromCatalog = true;

            Loader.unusedPromise = Loader.postData(this, requestBody, Loader.readUrl, id);
            this.displayed = true;
        }

        let checkboxes = [];
        if (this.state.data != null) {
            for (let i = 0; i < getStructuredTagsListResponse(this.state.data).length; i++) {
                checkboxes.push(<Checkbox key={`checkbox ${i}`} id={String(i)} jsonStorage={this.state.data} formId={undefined}/>);
            }
        }

        return (
            <div>
                <form ref={this.mainForm}
                    id="dizzy">
                    {checkboxes}
                </form>
                <div id="messageBox">
                    {this.state.data != null && getTextResponse(this.state.data) != null &&
                        <Message formId={this.formId} jsonStorage={this.state.data} id={undefined}/>
                    }
                </div>
            </div>
        );
    }
}

class Checkbox extends React.Component<ISimpleProps> {
    render() {
        let getTagName = (i: number) => {
            return getStructuredTagsListResponse(this.props.jsonStorage)[i];
        };
        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox" defaultChecked={false} />
                <label htmlFor={this.props.id}>{getTagName(Number(this.props.id))}</label>
            </div>
        );
    }
}

interface IWithLinks{
    text: string;
}

class NoteTextSupportLinks extends React.Component<IWithLinks> {

    constructor(props: IWithLinks) {
        super(props);
    }

    render() {
        // deprecated: JSX
        let res: (string|JSX.Element)[] = [];
        // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
        this.props.text && this.props.text.replace(
            /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim,
            (_: string, link: string, text: string):string => {
                res.push(link ? <a href={(link[0] === "w" ? "//" : "") + link} key={res.length}>{link}</a> : text);
                return "";
            })

        return <div className="user-text">{res}</div>
    }
}

class Message extends React.Component<ISimpleProps> {
    constructor(props: ISimpleProps) {
        super(props);
        this.hideMenu = this.hideMenu.bind(this);
    }

    hideMenu() {
        if (this.props.formId) {
            this.props.formId.style.display = toggleMenuVisibility(this.props.formId.style.display);
        }
    }

    render() {
        if (this.props.jsonStorage && Number(getCommonNoteId(this.props.jsonStorage)) !== 0) {
            window.textId = Number(getCommonNoteId(this.props.jsonStorage));
        }

        return (
            <span>
                {this.props.jsonStorage != null ? (getTextResponse(this.props.jsonStorage) != null ?
                    <span>
                        <div id="songTitle" onClick={this.hideMenu}>
                            { getTitleResponse(this.props.jsonStorage) }
                        </div>
                        <div id="songBody">
                            <NoteTextSupportLinks text={ getTextResponse(this.props.jsonStorage) ?? "" } />
                        </div>
                    </span>
                    : "выберите тег")
                    : ""}
            </span>
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
        (document.getElementById("login") as HTMLElement).style.display = "none";
        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        const item = {
            "tagsCheckedRequest": checkboxesArray
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(this.props.subscription, requestBody, Loader.readUrl);
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "slategrey";
    }

    componentWillUnmount() {
        // отменяй подписки и асинхронную загрузку
    }

    render() {
        return (
            <div id="submitStyle">
                <input form="dizzy" type="checkbox" id="submitButton" className="regular-checkbox" onClick={this.submit} />
                <label htmlFor="submitButton">Поиск</label>
            </div>
        );
    }
}
