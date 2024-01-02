import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { LoaderComponent } from "./loader.component.tsx";
import { menuHandler } from "../menu/menu.handler.tsx";
import { RouteComponentProps } from 'react-router';
import {
    getCommonNoteId,
    getStructuredTagsListResponse,
    getTextResponse,
    getTitleResponse
} from "../dto/dto.note.tsx";

interface IState {
    data: any;
}
interface IProps {
    subscription: any;
    formId: any;
    jsonStorage: any;
    id: any;
}

export class HomeView extends React.Component<RouteComponentProps<{textId: string}>, IState> {
    formId: any;
    mounted: boolean;
    displayed: boolean;

    readFromCatalog: boolean;

    public state: IState = {
        data: null
    }

    mainForm: React.RefObject<HTMLFormElement>;

    constructor(props: any) {
        // из "каталога" я попаду в конструктор, далее в DidMount, id песни { data: 1 } будет в props:
        super(props);

        this.formId = null;
        this.mounted = true;
        this.displayed = false;

        this.readFromCatalog = false;
        this.mainForm = React.createRef();
    }

    componentDidMount() {
        this.formId = this.mainForm.current;

        // если заметка вызвана из каталога, то не обновляем содержимое компонента:
        if (!this.readFromCatalog) {
            LoaderComponent.unusedPromise = LoaderComponent.getData(this, LoaderComponent.readUrl);
        }
        else{
            // убираем чекбоксы и логин если вызов произошёл из каталога:
            this.formId.style.display = "none";
            (document.getElementById("login")as HTMLElement).style.display = "none";
        }

        this.readFromCatalog = false;
    }

    componentDidUpdate() {
        ReactDOM.render(
            <div>
                <SubmitButton subscription={this} formId={this.formId} jsonStorage={null} id={null}/>
            </div>,
            document.querySelector("#searchButton1")
        );
        (document.getElementById("header")as HTMLElement).style.backgroundColor = "#e9ecee";//???
    }

    componentWillUnmount() {
        this.mounted = false;
        // убираем отображение кнопки "Поиск":
        ReactDOM.render(
            <div>
            </div>,
            document.querySelector("#searchButton1")
        );
    }

    render() {
        // читаем заметку из каталога:
        if (this.props.match.params.textId && !this.displayed) {
            console.log("Get text id from path params: " + this.props.match.params.textId);

            const item = {
                "tagsCheckedRequest": []
            };
            let requestBody = JSON.stringify(item);
            let id = this.props.match.params.textId;
            this.readFromCatalog = true;

            LoaderComponent.unusedPromise = LoaderComponent.postData(this, requestBody, LoaderComponent.readUrl, id);
            this.displayed = true;
        }

        let checkboxes = [];
        if (this.state.data != null) {
            for (let i = 0; i < getStructuredTagsListResponse(this.state.data).length; i++) {
                checkboxes.push(<Checkbox key={`checkbox ${i}`} id={i} jsonStorage={this.state.data} subscription={null} formId={null}/>);
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
                        <Message formId={this.formId} jsonStorage={this.state.data} subscription={null} id={null}/>
                    }
                </div>
            </div>
        );
    }
}

class Checkbox extends React.Component<IProps> {
    render() {
        let getTagName = (i: number) => {
            return getStructuredTagsListResponse(this.props.jsonStorage)[i];
        };
        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox" defaultChecked={false} />
                <label htmlFor={this.props.id}>{getTagName(this.props.id)}</label>
            </div>
        );
    }
}

interface IWithLinks{
    text: string;
}

class WithLinks extends React.Component<IWithLinks> {

    constructor(props: any) {
        super(props);
    }

    render() {
        let res: any = [];

        // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
        this.props.text && this.props.text.replace(
            /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim,
            (_, link, text) => {
            return res.push(link ? <a href={(link[0]==="w" ? "//" : "") + link} key={res.length}>{link}</a> : text);
        })

        return <div className="user-text">{res}</div>
    }
}

class Message extends React.Component<IProps> {
    constructor(props: any) {
        super(props);
        this.hideMenu = this.hideMenu.bind(this);
    }

    hideMenu() {
        this.props.formId.style.display = menuHandler(this.props.formId.style.display);
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
                            <WithLinks text={ getTextResponse(this.props.jsonStorage) } />
                        </div>
                    </span>
                    : "выберите тег")
                    : ""}
            </span>
        );
    }
}

class SubmitButton extends React.Component<IProps> {

    constructor(props: any) {
        super(props);
        this.submit = this.submit.bind(this);
    }

    submit(e: any) {
        e.preventDefault();
        (document.getElementById("login") as HTMLElement).style.display = "none";
        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        const item = {
            "tagsCheckedRequest": checkboxesArray
        };
        let requestBody = JSON.stringify(item);
        LoaderComponent.unusedPromise = LoaderComponent.postData(this.props.subscription, requestBody, LoaderComponent.readUrl);
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
