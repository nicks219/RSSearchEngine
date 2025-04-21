import * as React from "react";
import {useContext, useEffect} from "react";
import {ComponentMode, FunctionComponentStateWrapper} from "../common/state.handlers";
import {ComplianceResponseDto, NoteResponseDto} from "../dto/request.response.dto";
import {CommonContext} from "../common/context.provider";
import {getTextRequest, getTitleRequest} from "../common/dto.handlers";
import {Loader} from "../common/loader";
import {Doms, SystemConstants} from "../dto/doms.tsx";

export const CreateSubmitButton = (props: {formElement?: HTMLFormElement, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    // максимальное количество названий похожих заметок:
    const maxSimilarResultsTitleCount = 20;

    let jsonString: string = SystemConstants.empty;
    let similarNoteNameStorage: string[] = [];
    const similarNotesIdStorage: string[] = [];

    const commonContext = useContext(CommonContext);
    const cancel = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const buttonElement = (document.getElementById(Doms.cancelButton) as HTMLInputElement);
        buttonElement.style.display = SystemConstants.none;
        // always switch to classic mode:
        commonContext.componentMode = ComponentMode.Classic;

        // отмена: восстанавливаем текст и название заметки, при необходимости из внешнего стейта:
        if (jsonString == "") jsonString = commonContext.componentString;
        const text = getTextRequest(JSON.parse(jsonString));
        const title = getTitleRequest(JSON.parse(jsonString));
        jsonString = JSON.stringify({
            "tagsCheckedRequest": [],
            "textRequest": text,
            "titleRequest": title
        });

        Loader.unusedPromise = Loader.postData(props.stateWrapper, jsonString, Loader.createUrl);
    }

    const componentDidMount = () => {
        let buttonElement = (document.getElementById(Doms.cancelButton) as HTMLInputElement);
        buttonElement.style.display = SystemConstants.none;
    }
    const componentWillUnmount = () => {
        // перед выходом восстанавливаем состояние обёртки:
        commonContext.init();
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const submit = async (e: React.SyntheticEvent) => {
        e.preventDefault();

        const buttonElement = (document.getElementById(Doms.cancelButton) as HTMLInputElement);
        buttonElement.style.display = SystemConstants.none;

        // extended mode:
        if (commonContext.componentMode === ComponentMode.Extended) {
            // подтверждение: extended режим "подтверждение/отмена": при необходимости восстанавливаем заметку из внешнего стейта:
            commonContext.componentMode = ComponentMode.Classic;
            if (jsonString == SystemConstants.empty) jsonString = commonContext.componentString;
            Loader.unusedPromise = Loader.postData(props.stateWrapper, jsonString, Loader.createUrl);
            return;
        }

        // classic mode:
        const formData = new FormData(props.formElement);
        const checkboxesArray = (formData.getAll(Doms.chkButton)).map(a => Number(a) + 1);
        const formMessage = formData.get(Doms.msg);
        const formTitle = formData.get(Doms.ttl);
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": formTitle
        };
        jsonString = JSON.stringify(item);
        similarNoteNameStorage = [];
        await findSimilarNotes(formMessage, formTitle);
        // switch from classic to extended mode:
        if (similarNoteNameStorage.length > 0) {
            // сохраним requestBody во внешний стейт:
            commonContext.componentString = jsonString;
            buttonElement.style.display = SystemConstants.block;
            commonContext.componentMode = ComponentMode.Extended;
            return;
        }

        // совпадения не обнаружены, сохраняем заметку (classic режим):
        Loader.unusedPromise = Loader.postData(props.stateWrapper, jsonString, Loader.createUrl);
    }

    const findSimilarNotes = async (formMessage: FormDataEntryValue|null, formTitle: FormDataEntryValue|null) => {
        if (typeof formMessage === "string") {
            formMessage = formMessage.replace(/\r\n|\r|\n/g, " ");
        }

        const query = "?text=" + formMessage + " " + formTitle;
        const callback = (data: Response) => getNoteTitles(data);
        try {
            await Loader.getWithPromise(Loader.complianceIndicesUrl, query, callback);
        } catch (err) {
            console.log(`Find similar notes on create: ${err} in try-catch scope`);
        }
    }

    const getNoteTitles = async (response: Response) => {
        const responseDto = response as unknown as ComplianceResponseDto;
        const responseResult = responseDto.res;
        if (!responseResult) {
            return;
        }

        const array: number[][] = Object.keys(responseResult).map((key) => [Number(key), responseResult[Number(key)]]);
        array.sort(function (a, b) {
            return b[1] - a[1]
        });

        const result = [];
        for (let index in array) {
            result.push(array[index]);
        }

        if (result.length === 0) {
            return;
        }

        for (let index = 0; index < result.length; index++) {
            // прекращаем добавление похожих заметок к поисковой выдаче после превышения порога:
            if (similarNoteNameStorage.length >= maxSimilarResultsTitleCount) {
                continue;
            }

            // получаем имена возможных совпадений: id: string зто id заметки, можно вместо time его выставлять:
            const id = String(result[index][0]);
            const query = "?id=" + id;
            const callback = (data: Response) => getTitle(data, id);
            try {
                await Loader.getWithPromise(Loader.readTitleUrl, query, callback);
            } catch (err) {
                console.log(`Get note titles on create: ${err} in try-catch scope`);
            }
        }
    }

    const getTitle = (response: Response, id: string) => {
        const responseDto = response as unknown as ComplianceResponseDto;
        similarNoteNameStorage.push((responseDto.res + '\r\n'));
        similarNotesIdStorage.push(id);

        const data = {
            "structuredTagsListResponse": similarNoteNameStorage,
            "tagsCheckedUncheckedResponse": [],
            "textResponse": getTextRequest(JSON.parse(jsonString)),
            "titleResponse": getTitleRequest(JSON.parse(jsonString)),
            "tagIdsInternal": similarNotesIdStorage
        };
        props.stateWrapper.setData(data);
    }

    return (
        <div id={Doms.submitStyle}>
            <input type={Doms.checkbox} id={Doms.submitButton} className={Doms.regularCheckbox}/>
            <label htmlFor={Doms.submitButton} onClick={submit}>Создать</label>
            <div id={Doms.cancelButton}>
                <input type={Doms.checkbox} id={Doms.submitButtonDuplicate} className={Doms.regularCheckbox}/>
                <label htmlFor={Doms.submitButton} onClick={cancel}>Отмена</label>
            </div>
        </div>
    );
}
