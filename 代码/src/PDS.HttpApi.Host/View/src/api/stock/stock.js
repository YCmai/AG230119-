import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';

export function InSearch(input = {}) {
    return axios.get("/api/wms/material/getlist?" + url.normalize(input));
}

export function OutSearch(input = {}) {
    return axios.get("/api/wms/materialrecord/getlist?" + url.normalize(input));
}

export function Statistic(input = {}) {
    return axios.get("/api/wms/material/statistic?" + url.normalize(input))
}

export function Create(input) {
    return axios.post("/api/wms/material/ModifyAdd", input);
}

export function EditAvailableQuatity(input) {
    return axios.post("/api/wms/material/ModifyUpdateAvailCount", input);
}

export function SearchModifyRecord(input = {}) {
    return axios.get("/api/wms/material/GetModifyRecord?" + url.normalize(input));
}