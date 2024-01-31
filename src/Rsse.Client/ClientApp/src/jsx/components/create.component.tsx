import * as React from 'react';
import { Loader } from "./loader.tsx";
import {
    getCommonNoteId,
    getStructuredTagsListResponse,
    getTagsCheckedUncheckedResponse,
    getTextRequest,
    getTextResponse,
    getTitleRequest,
    getTitleResponse, setTextResponse, setTitleResponse
} from "../dto/handler.note.tsx";
import { ISimpleProps } from "../contracts/i.simple.props.tsx";
import { NoteResponseDto } from "../dto/note.response.dto.tsx";
import { ISubscribed } from "../contracts/i.subscribed.tsx";
import { IMountedComponent } from "../contracts/i.mounted.tsx";

interface IState {
    data?: NoteResponseDto;
    time: number|null;
    stateStorage?: string|null;
}

interface IProps extends ISimpleProps, ISubscribed<CreateView> {
}

class CreateView extends React.Component<ISimpleProps, IState> implements IMountedComponent {
    formId?: HTMLFormElement;
    mounted: boolean;

    public state: IState = {
        // NB использовать реальное время более корректно для key:
        time: null,
        stateStorage: null
    }

    mainForm: React.RefObject<HTMLFormElement>;

    constructor(props: IProps) {
        super(props);
        this.mounted = true;

        this.mainForm = React.createRef();
    }

    componentDidMount() {
        this.formId = this.mainForm.current ?? undefined;
        Loader.unusedPromise = Loader.getData(this, Loader.createUrl);
    }

    componentWillUnmount() {
        // переход на update при несохраненной заметке не приведёт к ошибке 400 (сервер не понимает NaN):
        if (isNaN(window.textId)) {
            window.textId = 0;
        }
        this.mounted = false;
    }

    componentDidUpdate() {
        if (this.state.stateStorage){
            console.log("Redirected: " + this.state.stateStorage);
            Loader.redirectToMenu("/#/read/" + this.state.stateStorage);
        }

        let id = 0;
        if (this.state.data) {
            id = Number(getCommonNoteId(this.state.data));
        }

        if (id !== 0) {
            window.textId = id;
        }
    }

    render() {
        let checkboxes = [];
        if (this.state.data != /*null*/undefined && getStructuredTagsListResponse(this.state.data) != null) {
            for (let i = 0; i < getStructuredTagsListResponse(this.state.data).length; i++) {
                checkboxes.push(<Checkbox key={`checkbox ${i}${this.state.time}`} id={String(i)} jsonStorage={this.state.data} /*subscription={null}*/ formId={undefined}/>);
            }
        }

        let jsonStorage = this.state.data;
        if (jsonStorage) {
            if (!getTextResponse(jsonStorage))
                setTextResponse(jsonStorage, "");
            if (!getTitleResponse(jsonStorage))
                setTitleResponse(jsonStorage, "");
        }

        return (
            <div id="renderContainer">
                <form ref={this.mainForm}
                    id="dizzy">
                    {checkboxes}
                    {this.state.data != null &&
                        <SubmitButton subscription={this} formId={this.formId} id={undefined} jsonStorage={undefined}/>
                    }
                </form>
                {this.state.data != null &&
                    <Message formId={this.formId} jsonStorage={jsonStorage} /*subscription={null}*/ id={undefined}/>
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

        let getGenreId = (i: number) => {
            if (this.props.jsonStorage?.structuredTagsListResponse !== undefined) {
                return this.props.jsonStorage.structuredTagsListResponse[i];
            }
            else {
                return "";
            }
        };

        return (
            <div id="checkboxStyle">
                <input name="chkButton" value={this.props.id} type="checkbox" id={this.props.id} className="regular-checkbox" defaultChecked={checked} />
                <label htmlFor={this.props.id} onClick={SubmitButton.loadNoteOnClick} about={getGenreId(Number(this.props.id))}>
                    {getGenreName(Number(this.props.id))}
                </label>
            </div>
        );
    }
}

class Message extends React.Component<ISimpleProps> {
    textHandler = (e: string) => {
        setTextResponse(this.props.jsonStorage, e);
        this.forceUpdate();
    }

    titleHandler = (e: string) => {
        setTitleResponse(this.props.jsonStorage, e);
        this.forceUpdate();
    }

    render() {
        return (
            <div >
                <p />
                {this.props.jsonStorage != null ?
                    <div>
                        <h5>
                            <textarea name="ttl" cols={66} rows={1} form="dizzy"
                                value={ getTitleResponse(this.props.jsonStorage) } onChange={e => this.titleHandler(e.target.value)} />
                        </h5>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                value={ getTextResponse(this.props.jsonStorage) } onChange={e => this.textHandler(e.target.value)} />
                        </h5>
                    </div>
                    : "loading.."}
            </div>
        );
    }
}

class SubmitButton extends React.Component<IProps> {

    requestBody: string = "";
    storage: string[] = [];
    storageId: string[] = [];
    submitState: number;
    static state?: number;
    static subscriber: CreateView;

    constructor(props: IProps) {
        super(props);
        this.submit = this.submit.bind(this);
        // подтверждение или отмена:
        this.submitState = 0;
        SubmitButton.subscriber = this.props.subscription;
    }

    // чекбоксы превращаются в ссылки на каталог заметок:
    static loadNoteOnClick = (e: React.SyntheticEvent) => {
        if (SubmitButton.state !== undefined) {

            let title = e.currentTarget.innerHTML.valueOf();
            let id = e.currentTarget.attributes.item(1)?.nodeValue;

            // subscription на компонент create.
            console.log("Submitted: " + SubmitButton.state + " " + title + " " + id);
            SubmitButton.subscriber.setState({stateStorage: id});
        }
    }

    cancel = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let buttonElement  = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";

        // отмена - сохраняем текст и название:
        SubmitButton.state = undefined;
        this.submitState = 0;

        let text = getTextRequest(JSON.parse(this.requestBody));
        let title = getTitleRequest(JSON.parse(this.requestBody));

        this.requestBody = JSON.stringify({
            "tagsCheckedRequest":[],
            "textRequest": text,
            "titleRequest": title
            });
        Loader.unusedPromise = Loader.postData(this.props.subscription, this.requestBody, Loader.createUrl);
    }

    componentDidMount() {
        let buttonElement  = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
    }

    async submit(e: React.SyntheticEvent) {
        e.preventDefault();

        let buttonElement  = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
        SubmitButton.state = this.submitState;

        if (this.submitState === 1)
        {
            // подтверждение:
            SubmitButton.state = undefined;

            this.submitState = 0;
            Loader.unusedPromise = Loader.postData(this.props.subscription, this.requestBody, Loader.createUrl);
            return;
        }

        let formData = new FormData(this.props.formId);
        let checkboxesArray = (formData.getAll('chkButton')).map(a => Number(a) + 1);
        let formMessage = formData.get('msg');
        let formTitle = formData.get('ttl');
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": formTitle
        };
        this.requestBody = JSON.stringify(item);

        this.storage = [];
        let promise = this.findSimilarNotes(formMessage, formTitle);
        await promise;

        if (this.storage.length > 0)
        {
            // переключение в "подтверждение или отмена":
            buttonElement.style.display = "block";
            this.submitState = 1;
            return;
        }

        // совпадения не обнаружены:
        Loader.unusedPromise = Loader.postData(this.props.subscription, this.requestBody, Loader.createUrl);
    }

    findSimilarNotes = async (formMessage: string | File | null, formTitle: string | File | null) => {
        let promise;

        if (typeof formMessage === "string") {
            formMessage = formMessage.replace(/\r\n|\r|\n/g, " ");
        }

        let callback = (data: Response) => this.getNoteTitles(data);
        let query = "?text=" + formMessage + " " + formTitle;

        try {
            promise = Loader.getWithPromise(Loader.complianceIndicesUrl, query, callback);
        } catch (err) {
            console.log("Find when create: try-catch err");
        }

        if (promise !== undefined) {
            await promise;}
    }

    getNoteTitles = async (data: Response) => {
        let responseDto = data as unknown as ComplianceResponseDto;
        let response = responseDto.res;
        if (response === undefined) {
            return;
        }

        //let item = response[1];
        //let r = item["1"];
        let array: number[][] = Object.keys(response).map((key) => [Number(key), response[Number(key)]]);
        array.sort(function (a, b) {
            return b[1] - a[1]
        });

        let result = [];
        for (let index in array) {
            result.push(array[index]);
        }

        if (result.length === 0) {
            return;
        }

        for (let ind = 0; ind < result.length; ind++) {
            // лучше сделать reject:
            if (this.storage.length >= 10) {
                continue;
            }

            let i = String(result[ind][0]);

            // получаем имена возможных совпадений: i:string зто id заметки, можно вместо time его выставлять:
            let promise;

            let callback = (data: Response) => this.getTitle(data, i);

            let query = "?id=" + i;

            try {
                promise = Loader.getWithPromise(Loader.readTitleUrl, query, callback);
            } catch (err) {
                console.log("Find when create: try-catch err");
            }

            if (promise !== undefined) {
                await promise;
            }
        }
    }

    getTitle = (input: Response, i: string) => {// в поле data.res сидит string: (new {res}) | any: data.res
        let responseDto = input as unknown as ComplianceResponseDto;
        this.storage.push((responseDto.res + '\r\n'));
        this.storageId.push(i);

        // stub:
        let data = {
            "structuredTagsListResponse": this.storage,
            "tagsCheckedUncheckedResponse": [],
            "textResponse": getTextRequest(JSON.parse(this.requestBody)),
            "titleResponse": getTitleRequest(JSON.parse(this.requestBody)),
            "genresNamesId": this.storageId
        };
        let time = Date.now();
        // subscription на CreateView:
        this.props.subscription.setState({ data , time });
    }

    componentWillUnmount() {
        // TODO отменить подписки и асинхронную загрузку
    }

    render() {
        return (
            <div id="submitStyle">
                <input type="checkbox" id="submitButton" className="regular-checkbox" />
                <label htmlFor="submitButton" onClick={this.submit}>Создать</label>
                  <div id="cancelButton">
                    <input type="checkbox" id="submitButtonDuplicate" className="regular-checkbox" />
                    <label htmlFor="submitButton" onClick={this.cancel}>Отменить</label>
                  </div>
            </div>
        );
    }
}

class ComplianceResponseDto {
    res: {[key: number]: number} = [];
}

export default CreateView;
