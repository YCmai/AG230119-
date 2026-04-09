import axios from '@/lib/axios-extension';
export function BindOperation(input) {

    return axios.post("/api/task/padagvtask/bindOperation", input)
}