<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions style="text-align: left" :column="4">
        <a-descriptions-item label="储位名称">
          <a-input></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="所属仓库">
          <a-select
            class="form-model-item-select"
            v-model="query.wareHouseCode"
            @change="selectWareHouse"
          >
            <a-select-option value="">--全部仓库--</a-select-option>
            <a-select-option
              v-for="(ware, index) in wares"
              :key="index"
              :value="ware.id"
              >{{ ware.id }}</a-select-option
            >
          </a-select>
        </a-descriptions-item>
        <a-descriptions-item label="所属区域">
          <a-select class="form-model-item-select" v-model="query.areaCode">
            <a-select-option value="">--全部区域--</a-select-option>
            <a-select-option
              v-for="(area, index) in areas"
              :key="index"
              :value="area.code"
              >{{ area.code }}</a-select-option
            >
          </a-select>
        </a-descriptions-item>
        <a-descriptions-item class="text-align-right">
          <a-button type="primary" icon="search" size="small" @click="load">
            搜索
          </a-button>
        </a-descriptions-item>
      </a-descriptions>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-row class="layout-content-operation">
        <a-row class="layout-content-operation" style="margin-bottom: 3%;">
        <a-col :span="6">
          <a-button  size="small" type="primary" @click="updateNode(1)">启用储位</a-button>
        </a-col>
        <a-col :span="6">
          <a-button  size="small" type="primary" @click="updateNode(2)">禁用储位</a-button>
        </a-col>
        <a-col :span="6">
          <a-button  size="small" type="primary" @click="updateNode(3)">清空储位</a-button>
        </a-col>
        <a-col :span="5">
          <a-button  size="small" type="primary" @click="show(null)">新增储位</a-button>
        </a-col>
      </a-row>
      </a-row>
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.id"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
        @expand="expand"
      >
        <template slot="operation" slot-scope="text, record">
          <a-space>
            <a href="javascript:void(0);" @click="show(null, record.id)">
              <a-icon type="plus-circle" />新增
            </a>
            <a href="javascript:void(0);" @click="show(record.id)">
              <a-icon type="edit" />{{ $t("actions.edit") }}
            </a>
            <a href="javascript:void(0);" @click="remove(record.id)">
              <a-icon type="edit" />{{ $t("actions.delete") }}
            </a>
          </a-space>
        </template>
      </a-table>
      <store-form-vue v-if="showForm" @close="close" ref="form" />
    </a-layout-content>
  </a-layout>
</template>
<script>
import locale from "@/localize/store/locale";
import StoreFormVue from "./Form.vue";
import { BindOperation } from "@/api/task/agvtask";
import { GetStores, GetChildStores, DeleteStore } from "@/api/inventory/store";
import { GetAllWareHouses } from "@/api/inventory/wareHouse";
import component from "@/lib/base";
export default {
  name: "StoreList",
  i18n: {
    messages: locale,
  },
  mixins: [component],
  components: { StoreFormVue },
  data() {
    return {
      loading: false,
      pagination: {
        total: 0,
        current: 1,
        pageSize: 10,
        "show-total": (total) => `共${total}条数据`,
        "show-size-changer": true,
      },
      query: {
        wareHouseCode: "",
        areaCode: "",
      },
      data: [],
      params: {
        selectedRowKeys: [], // 存储选中的行的key
        value: 0
      },
      selectedRowKeys: [],
      columns: [
        
        {
          dataIndex: "id",
          title: "储位编码",
          align: "center",
        },
        {
          dataIndex: "name",
          title: "储位名称",
          align: "center",
        },
        {
          dataIndex: "wareHouseId",
          title: "库别编码",
          align: "center",
        },
        {
          dataIndex: "wareHouseIdAreaCode",
          title: "区域编码",
          align: "center",
        },
        {
          dataIndex: "category",
          title: "储位分类",
          align: "center",
        },
        {
          dataIndex: "mapNodeName",
          title: "地图节点",
          align: "center",
        },
        {
          dataIndex: "currentNodeMaterialCount",
          title: "物料数量",
          align: "center",
        },
        {
          dataIndex: "appData1",
          title: "留用数据1",
          align: "center",
        },
        {
          dataIndex: "appData2",
          title: "留用数据2",
          align: "center",
        },
        {
          dataIndex: "operations",
          title: this.$t("title.actions"),
          align: "center",
          scopedSlots: { customRender: "operation" },
          width: 200,
        },
      ],
      wares: [],
      areas: [],
      showForm: false,
    };
  },
  mounted() {
    this.init();
    this.load();
  },
  methods: {
    init: async function () {
      try {
        let wares = await GetAllWareHouses();
        this.wares = wares;
      } catch (err) {
        console.log(err);
      }
    },
    updateNode: async function (value) {
     let _this = this;
     this.params.value = value;
     if (this.params.selectedRowKeys == "" || this.params.selectedRowKeys == null) {
       _this.$message.error("请先选择储位");
       return;
     }
     const input = {
     selectedRowKeys: this.params.selectedRowKeys,
     value: value
     };
     this.$confirm({
       content: '是否执行储位批量操作？',
       onOk() {
        try {
          BindOperation(input);
        _this.$message.success("更新成功");
        } catch (err) {
          _this.$message.error(err.code);
        }
       
       },
       onCancel() { },
     });
   },
   selectshow: function () {
      if (this.selectedRowKeys.length == 0) {
        this.$message.error("请先选择储位!");
        return;
      }
      this.selectForm = true;
      this.$nextTick(() => {
        this.$refs.nform.init(this.selectedRowKeys);
      });
    },
    load: async function () {
      this.loading = true;
      try {
        let query = Object.assign(this.query, {
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        let result = await GetStores(query);
        let data = result.items.map((m) => {
          return Object.assign(m, { children: [] });
        });
        this.data = data;
        this.pagination.total = result.totalCount;
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
    show: function (id, parent) {
      this.showForm = true;
      this.$nextTick(() => {
        this.$refs.form.init(id, parent);
      });
    },
    close: function () {
      this.showForm = false;
      this.showForm = false;
      this.selectedRowKeys = [];  // 重置选中的行
      this.tableKey += 1; // 强制刷新表格
      this.$forceUpdate(); // 强制更新组件
      this.load();
    },
    onSelectChange(selectedRowKeys, selectedRows) {
      this.selectedRowKeys = selectedRowKeys;
      this.params.selectedRowKeys = selectedRowKeys;
      console.log('selectedRowKeys changed: ', selectedRowKeys);
      console.log('selectedRows changed: ', selectedRows);
    },
    expand: async function (isOpen, record) {
      if (!isOpen) return;
      this.loading = true;
      try {
        let query = Object.assign({ parentStorageId: record.id }, this.query);
        let result = await GetChildStores(query);
        record.children = result.items.map((m) => {
          return Object.assign(m, { children: [] });
        });
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    selectWareHouse: function (value) {
      this.query.areaCode = "";
      if (!value) {
        this.areas = [];
        return;
      }
      let wareHouse = this.wares.filter((m) => {
        return m.id == value;
      })[0];
      if (wareHouse) this.areas = wareHouse.areas;
    },
    remove: async function (id) {
      if (!(await this.confirm("确定删除储位?"))) return;
      try {
        await DeleteStore(id);
        this.$message.success("删除成功!");
        this.load();
      } catch (err) {
        console.log(err.message);
      }
    },
  },
};
</script>
