<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions :column="4">
        <a-descriptions-item label="SKU编码">
          <a-input></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="物料名称">
          <a-input></a-input>
        </a-descriptions-item>
        <a-descriptions-item>
          <a-button type="primary" icon="search" size="small" @click="load">
            搜索
          </a-button>
        </a-descriptions-item>
      </a-descriptions>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-row class="layout-content-operation">
        <a-col :span="24">
          <a-button icon="plus" size="small" type="primary" @click="show(null)">
            {{ $t("actions.add") }}
          </a-button>
        </a-col>
      </a-row>
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.sku"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      >
        <template slot="isContainer" slot-scope="text, record">
          {{ record.isContainer ? "是" : "否" }}
        </template>
        <template slot="operation" slot-scope="text, record">
          <a-space>
            <a href="javascript:void(0);" @click="show(record.sku)">
              <a-icon type="edit" />{{ $t("actions.edit") }}
            </a>
            <a href="javascript:void(0);" @click="remove(record.sku)">
              <a-icon type="edit" />{{ $t("actions.delete") }}
            </a>
          </a-space>
        </template>
      </a-table>
    </a-layout-content>
    <material-form-vue v-if="showForm" ref="materialForm" @close="close" />
  </a-layout>
</template>
<script>
import MaterialFormVue from "./Form.vue";
import { GetPage, Remove } from "@/api/materials/material";
import component from "@/lib/base";
export default {
  name: "MaterialList",
  mixins: [component],
  components: { MaterialFormVue },
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
      columns: [
        {
          dataIndex: "sku",
          title: "SKU编码",
          align: "center",
        },
        {
          dataIndex: "name",
          title: "物料名称",
          align: "center",
        },
        {
          dataIndex: "sizeMess",
          title: "物料尺寸",
          align: "center",
        },
        {
          dataIndex: "isContainer",
          title: "是否容器",
          align: "center",
          scopedSlots: { customRender: "isContainer" },
        },
        {
          dataIndex: "describtion",
          title: "描述",
          align: "center",
        },
        {
          dataIndex: "actions",
          title: "操作",
          align: "center",
          scopedSlots: { customRender: "operation" },
          width: 200,
        },
      ],
      data: [],
      showForm: false,
      query: {},
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      this.loading = true;
      try {
        let query = Object.assign(this.query, {
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        let result = await GetPage(query);
        this.data = result.items;
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
    show: function (id) {
      this.showForm = true;
      if (id)
        this.$nextTick(() => {
          this.$refs.materialForm.load(id);
        });
    },
    close: function () {
      this.showForm = false;
      this.load();
    },
    remove: async function (sku) {
      try {
        await Remove(sku);
      } catch (err) {
        this.$message.error(err.message);
      }
    },
  },
};
</script>
